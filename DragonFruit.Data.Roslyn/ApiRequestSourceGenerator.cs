﻿// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using DragonFruit.Data.Requests;
using DragonFruit.Data.Roslyn.Entities;
using DragonFruit.Data.Roslyn.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace DragonFruit.Data.Roslyn
{
    [Generator(LanguageNames.CSharp)]
    public class ApiRequestSourceGenerator : IIncrementalGenerator
    {
        public static readonly string TemplateName = "DragonFruit.Data.Roslyn.Templates.ApiRequest.liquid";

        private static readonly HashSet<SpecialType> SupportedCollectionTypes =
        [
            ..new[]
            {
                SpecialType.System_Array,
                SpecialType.System_Collections_IEnumerable,
                SpecialType.System_Collections_Generic_IList_T,
                SpecialType.System_Collections_Generic_ICollection_T,
                SpecialType.System_Collections_Generic_IEnumerable_T,
                SpecialType.System_Collections_Generic_IReadOnlyList_T,
                SpecialType.System_Collections_Generic_IReadOnlyCollection_T
            }
        ];

        private Template _partialRequestTemplate;

        static ApiRequestSourceGenerator()
        {
            ExternalDependencyLoader.RegisterDependencyLoader();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            using var templateStream = typeof(ApiRequestSourceGenerator).Assembly.GetManifestResourceStream(TemplateName);

            if (templateStream == null)
            {
                throw new NullReferenceException("Could not find template");
            }

            using var reader = new StreamReader(templateStream);
            _partialRequestTemplate = Template.ParseLiquid(reader.ReadToEnd());

            var apiRequestDerivedClasses = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax classDecl && classDecl.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)),
                transform: (generatorSyntaxContext, _) => GetSemanticTarget(generatorSyntaxContext));

            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> targets = context.CompilationProvider.Combine(apiRequestDerivedClasses.Collect());
            context.RegisterSourceOutput(targets, (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> requestClasses, SourceProductionContext context)
        {
            if (requestClasses.IsDefaultOrEmpty)
            {
                return;
            }

            var httpContentSymbol = compilation.GetTypeByMetadataName(typeof(HttpContent).FullName);

            foreach (var classDeclaration in requestClasses.Distinct())
            {
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree, true);
                var classSymbol = model.GetDeclaredSymbol(classDeclaration)!;

                var sourceBuilder = new StringBuilder("// <auto-generated />");
                var metadata = GetRequestSymbolMetadata(compilation, classSymbol);

                // check if body is derived from httpcontent
                var requestBodyType = RequestBodyType.None;

                if (metadata.BodyProperty != null)
                {
                    requestBodyType = DerivesFrom(metadata.BodyProperty.ReturnType, httpContentSymbol) ? RequestBodyType.CustomBodyDirect : RequestBodyType.CustomBodySerialized;
                }
                else if (metadata.Properties[ParameterType.Form].Any())
                {
                    requestBodyType = metadata.FormBodyType == FormBodyType.MultipartForm ? RequestBodyType.FormMultipart : RequestBodyType.FormUriEncoded;
                }

                var parameterInfo = new
                {
                    ClassName = classSymbol.Name,
                    Namespace = classSymbol.ContainingNamespace.ToDisplayString(),

                    RequestBodyType = requestBodyType,
                    RequestBodySymbol = metadata.BodyProperty,

                    QueryParameters = metadata.Properties[ParameterType.Query],
                    HeaderParameters = metadata.Properties[ParameterType.Header],
                    FormBodyParameters = metadata.Properties[ParameterType.Form],
                };

                sourceBuilder.Append("\n\n");
                sourceBuilder.Append(_partialRequestTemplate.Render(parameterInfo));
                context.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
        }

        private static ClassDeclarationSyntax GetSemanticTarget(GeneratorSyntaxContext context)
        {
            var model = context.SemanticModel;
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            var apiRequestSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(ApiRequest).FullName);
            var classSymbol = ModelExtensions.GetDeclaredSymbol(model, classDeclaration) as INamedTypeSymbol;

            // ensure the class isn't abstract
            if (classSymbol?.IsAbstract != false)
            {
                return null;
            }

            // don't allow processing nested classes
            if (classSymbol.ContainingType != null)
            {
                return null;
            }

            // check if class is derived from ApiRequest
            while (classSymbol != null)
            {
                if (classSymbol.Equals(apiRequestSymbol, SymbolEqualityComparer.Default))
                {
                    return classDeclaration;
                }

                classSymbol = classSymbol.BaseType;
            }

            return null;
        }

        private static RequestSymbolMetadata GetRequestSymbolMetadata(Compilation compilation, INamedTypeSymbol symbol)
        {
            var metadata = new RequestSymbolMetadata
            {
                Properties = Enum.GetValues(typeof(ParameterType)).Cast<ParameterType>().ToDictionary(x => x, _ => (IList<SymbolMetadata>)new List<SymbolMetadata>())
            };

            // get types used in member processing
            var enumerableParameterAttribute = compilation.GetTypeByMetadataName(typeof(EnumerableOptionsAttribute).FullName);
            var requestParameterAttribute = compilation.GetTypeByMetadataName(typeof(RequestParameterAttribute).FullName);
            var enumParameterAttribute = compilation.GetTypeByMetadataName(typeof(EnumOptionsAttribute).FullName);
            var requestBodyAttribute = compilation.GetTypeByMetadataName(typeof(RequestBodyAttribute).FullName);
            var formBodyTypeAttribute = compilation.GetTypeByMetadataName(typeof(FormBodyTypeAttribute).FullName);

            var enumerableTypeSymbol = compilation.GetTypeByMetadataName(typeof(IEnumerable).FullName);
            var apiRequestBaseType = compilation.GetTypeByMetadataName(typeof(ApiRequest).FullName);
            var streamTypeSymbol = compilation.GetTypeByMetadataName(typeof(Stream).FullName);

            // track properties already visited
            var depth = 0;
            var currentSymbol = symbol;
            var consumedProperties = new HashSet<string>();

            do
            {
                // check for body type attribute
                var formBodyInfo = currentSymbol.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Equals(formBodyTypeAttribute, SymbolEqualityComparer.Default) == true);

                if (formBodyInfo != null)
                {
                    metadata.FormBodyType ??= (FormBodyType)formBodyInfo.ConstructorArguments[0].Value!;
                }

                // locate and add symbol metadata
                foreach (var candidate in currentSymbol.GetMembers().Where(x => x is IPropertySymbol or IMethodSymbol { Parameters.Length: 0 }))
                {
                    var requestAttribute = candidate.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Equals(requestParameterAttribute, SymbolEqualityComparer.Default) == true);

                    // ensure properties overwritten using "new" are not processed twice
                    if (requestAttribute == null || !consumedProperties.Add(candidate.MetadataName))
                    {
                        continue;
                    }

                    // only allow public, protected and protected internal properties
                    if (candidate.DeclaredAccessibility is Accessibility.Private or Accessibility.Internal)
                    {
                        // todo diagnostic warning
                        continue;
                    }

                    var returnType = candidate switch
                    {
                        IPropertySymbol propertySymbol => propertySymbol.Type,
                        IMethodSymbol methodSymbol => methodSymbol.ReturnType,

                        _ => throw new NotSupportedException()
                    };

                    // if a method is declared, ensure it's not void
                    if (returnType.SpecialType == SpecialType.System_Void)
                    {
                        // todo return diagnostic warning
                        continue;
                    }

                    // check if value is decorated with RequestBodyAttribute
                    if (metadata.BodyProperty != null && candidate.GetAttributes().Any(x => x.AttributeClass?.Equals(requestBodyAttribute, SymbolEqualityComparer.Default) == true))
                    {
                        metadata.BodyProperty = new SymbolMetadata(candidate, returnType);
                    }

                    var parameterType = (ParameterType)requestAttribute.ConstructorArguments[0].Value!;
                    var parameterName = (string)requestAttribute.ConstructorArguments.ElementAtOrDefault(1).Value ?? candidate.Name;

                    SymbolMetadata symbolMetadata;

                    // handle enums
                    if (returnType.TypeKind == TypeKind.Enum)
                    {
                        var enumOptions = candidate.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Equals(enumParameterAttribute, SymbolEqualityComparer.Default) == true);
                        var enumType = (EnumOption?)enumOptions?.ConstructorArguments.ElementAt(0).Value ?? EnumOption.None;

                        symbolMetadata = new EnumSymbolMetadata(candidate, returnType, parameterName)
                        {
                            EnumOption = enumType.ToString()
                        };
                    }
                    // handle arrays/IEnumerable
                    else if (SupportedCollectionTypes.Contains(returnType.SpecialType) || returnType.FindImplementationForInterfaceMember(enumerableTypeSymbol) != null)
                    {
                        var enumerableOptions = candidate.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Equals(enumerableParameterAttribute, SymbolEqualityComparer.Default) == true);
                        var enumerableType = (EnumerableOption?)enumerableOptions?.ConstructorArguments.ElementAt(0).Value ?? EnumerableOption.Concatenated;

                        symbolMetadata = new EnumerableSymbolMetadata(candidate, returnType, parameterName)
                        {
                            Separator = (string)enumerableOptions?.ConstructorArguments.ElementAtOrDefault(1).Value ?? ",",
                            EnumerableOption = enumerableType.ToString()
                        };
                    }
                    else
                    {
                        var psm = new PropertySymbolMetadata(candidate, returnType, parameterName);

                        if (DerivesFrom(returnType, streamTypeSymbol))
                        {
                            psm.SpecialRequestParameter = SpecialRequestParameter.Stream;
                        }
                        else if (returnType is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte })
                        {
                            psm.SpecialRequestParameter = SpecialRequestParameter.ByteArray;
                        }

                        symbolMetadata = psm;
                    }

                    symbolMetadata.Depth = depth;

                    metadata.Properties[parameterType].Add(symbolMetadata);
                }

                // get derived class from currentSymbol
                currentSymbol = currentSymbol.BaseType;
                depth++;
            } while (currentSymbol?.Equals(apiRequestBaseType, SymbolEqualityComparer.Default) == false);

            return metadata;
        }

        private static bool DerivesFrom(ITypeSymbol type, ITypeSymbol baseType)
        {
            var classSymbol = type;

            while (classSymbol != null)
            {
                if (classSymbol.Equals(baseType, SymbolEqualityComparer.Default))
                {
                    return true;
                }

                classSymbol = classSymbol.BaseType;
            }

            return false;
        }
    }
}
