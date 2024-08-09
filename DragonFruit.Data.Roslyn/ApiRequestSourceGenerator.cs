// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using DragonFruit.Data.Converters;
using DragonFruit.Data.Requests;
using DragonFruit.Data.Roslyn.Entities;
using DragonFruit.Data.Roslyn.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DragonFruit.Data.Roslyn
{
    [Generator(LanguageNames.CSharp)]
    public class ApiRequestSourceGenerator : IIncrementalGenerator
    {
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

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var apiRequestDerivedClasses = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax classDecl && classDecl.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)),
                transform: (generatorSyntaxContext, _) => GetSemanticTarget(generatorSyntaxContext));

            var targets = context.CompilationProvider.Combine(apiRequestDerivedClasses.Where(x => x != null).Collect());
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

                var metadata = GetRequestSymbolMetadata(compilation, classSymbol);

                // check if body is derived from httpcontent
                var requestBodyType = RequestBodyType.None;

                if (metadata.BodyProperty != null)
                {
                    var isHttpContent = metadata.BodyProperty.ReturnType.Equals(httpContentSymbol, SymbolEqualityComparer.Default) || DerivesFrom(metadata.BodyProperty.ReturnType, httpContentSymbol);
                    requestBodyType = isHttpContent ? RequestBodyType.CustomBodyDirect : RequestBodyType.CustomBodySerialized;
                }
                else if (metadata.Properties[ParameterType.Form].Any())
                {
                    requestBodyType = metadata.FormBodyType == FormBodyType.Multipart ? RequestBodyType.FormMultipart : RequestBodyType.FormUriEncoded;
                }

                var requireNewKeyword = WillHideOtherMembers(classSymbol, compilation.GetTypeByMetadataName(typeof(ApiRequest).FullName));
                context.AddSource($"{classSymbol.Name}.dragonfruit.g.cs", ApiRequestSourceBuilder.Build(classSymbol, metadata, requestBodyType, requireNewKeyword));
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

        /// <summary>
        /// Collects information from classes regarding properties and methods decorated with <see cref="RequestParameterAttribute"/> and <see cref="RequestBodyAttribute"/> to use in source generation.
        /// Applies inheritance rules to ensure that candidates are not duplicated.
        /// </summary>
        private static RequestSymbolMetadata GetRequestSymbolMetadata(Compilation compilation, INamedTypeSymbol symbol)
        {
            var metadata = new RequestSymbolMetadata
            {
                Properties = Enum.GetValues(typeof(ParameterType)).Cast<ParameterType>().ToDictionary(x => x, _ => new List<SymbolMetadata>())
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

            // create IEnumerable<KeyValuePair<string, string>> impl
            var stringTypeSymbol = compilation.GetSpecialType(SpecialType.System_String);
            var constructedKeyValuePairTypeSymbol = compilation.GetTypeByMetadataName(typeof(KeyValuePair<,>).FullName)!.Construct(stringTypeSymbol, stringTypeSymbol);
            var keyValuePairEnumerableTypeSymbol = compilation.GetTypeByMetadataName(typeof(IEnumerable<>).FullName)!.Construct(constructedKeyValuePairTypeSymbol);

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
                    var parameterAttribute = candidate.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Equals(requestParameterAttribute, SymbolEqualityComparer.Default) == true);
                    var bodyAttribute = candidate.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Equals(requestBodyAttribute, SymbolEqualityComparer.Default) == true);

                    // ensure properties overwritten using "new" are not processed twice
                    if ((parameterAttribute == null && bodyAttribute == null) || !consumedProperties.Add(candidate.MetadataName))
                    {
                        continue;
                    }

                    // only allow public, protected and protected internal properties
                    if (candidate.DeclaredAccessibility is Accessibility.Private or Accessibility.Internal)
                    {
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
                        continue;
                    }

                    // check if property and has a getter
                    var candidateMethod = candidate switch
                    {
                        IPropertySymbol propertySymbol => propertySymbol.GetMethod,
                        IMethodSymbol methodSymbol => methodSymbol,

                        _ => throw new NotSupportedException()
                    };

                    if (candidateMethod == null)
                    {
                        continue;
                    }

                    // inherited properties that are private or internal are ignored
                    if (depth > 0 && candidateMethod.DeclaredAccessibility is Accessibility.Private or Accessibility.Internal)
                    {
                        continue;
                    }

                    // check if value is decorated with RequestBodyAttribute
                    if (bodyAttribute != null && metadata.BodyProperty == null)
                    {
                        metadata.BodyProperty = new SymbolMetadata(candidate, returnType);
                        continue;
                    }

                    SymbolMetadata symbolMetadata;

                    var parameterType = (ParameterType)parameterAttribute.ConstructorArguments[0].Value!;
                    var parameterName = (string)parameterAttribute.ConstructorArguments.ElementAtOrDefault(1).Value ?? candidate.Name;

                    // handle enums
                    if (returnType.TypeKind == TypeKind.Enum)
                    {
                        var enumOptions = candidate.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Equals(enumParameterAttribute, SymbolEqualityComparer.Default) == true);

                        symbolMetadata = new EnumSymbolMetadata(candidate, returnType, parameterName)
                        {
                            EnumOption = enumOptions != null ? (EnumOption)enumOptions.ConstructorArguments.ElementAt(0).Value : EnumOption.None
                        };
                    }
                    // string (IEnumerable<char>)
                    else if (returnType.SpecialType == SpecialType.System_String)
                    {
                        symbolMetadata = new ParameterSymbolMetadata(candidate, returnType, parameterName);
                    }
                    // Stream
                    else if (streamTypeSymbol.Equals(returnType, SymbolEqualityComparer.Default) || DerivesFrom(returnType, streamTypeSymbol))
                    {
                        symbolMetadata = new ParameterSymbolMetadata(candidate, returnType, parameterName, RequestSymbolType.Stream);
                    }
                    else
                    {
                        // other enumerable types (and default)
                        switch (SupportedCollectionTypes.Contains(returnType.SpecialType) || returnType.AllInterfaces.Any(x => x.Equals(enumerableTypeSymbol, SymbolEqualityComparer.Default)))
                        {
                            // byte[]
                            case true when returnType is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte }:
                                symbolMetadata = new ParameterSymbolMetadata(candidate, returnType, parameterName, RequestSymbolType.ByteArray);
                                break;

                            // IEnumerable<KeyValuePair<string, string>>
                            case true when returnType.AllInterfaces.Any(x => x.Equals(keyValuePairEnumerableTypeSymbol, SymbolEqualityComparer.Default)):
                                symbolMetadata = new KeyValuePairSymbolMetadata(candidate, returnType, parameterName);
                                break;

                            // IEnumerable
                            case true:
                            {
                                var enumerableOptions = candidate.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Equals(enumerableParameterAttribute, SymbolEqualityComparer.Default) == true);

                                symbolMetadata = new EnumerableSymbolMetadata(candidate, returnType, parameterName)
                                {
                                    Separator = (string)enumerableOptions?.ConstructorArguments.ElementAtOrDefault(1).Value ?? EnumerableConverter.DefaultSeparator,
                                    EnumerableOption = enumerableOptions != null ? (EnumerableOption)enumerableOptions.ConstructorArguments.ElementAt(0).Value : EnumerableConverter.DefaultOption
                                };
                                break;
                            }

                            default:
                                symbolMetadata = new ParameterSymbolMetadata(candidate, returnType, parameterName);
                                break;
                        }
                    }

                    symbolMetadata.Depth = depth;
                    metadata.Properties[parameterType].Add(symbolMetadata);
                }

                // get derived class from currentSymbol
                currentSymbol = currentSymbol.BaseType;
                depth++;
            } while (currentSymbol?.Equals(apiRequestBaseType, SymbolEqualityComparer.Default) == false);

            // reverse by depth to put base properties first but retain order within each depth
            foreach (var list in metadata.Properties.Values)
            {
                list.Sort((a, b) => b.Depth - a.Depth);
            }

            return metadata;
        }

        /// <summary>
        /// Determines if a type derives from another type
        /// </summary>
        /// <param name="type">The <see cref="ITypeSymbol"/> to check</param>
        /// <param name="baseType">The <see cref="ITypeSymbol"/> the <see cref="type"/> is supposed to inherit from</param>
        /// <returns><c>true</c> if <see cref="type"/> derives from <see cref="baseType"/>, else <c>false</c></returns>
        internal static bool DerivesFrom(ITypeSymbol type, ITypeSymbol baseType)
        {
            var classSymbol = type.BaseType;

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

        /// <summary>
        /// Determines whether the specified type will overwrite any generated methods.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <param name="baseType">The type to stop checking at</param>
        /// <returns><c>true</c> if the type will generate a member that hides a base implementation, otherwise <c>false</c></returns>
        private static bool WillHideOtherMembers(ITypeSymbol type, ISymbol baseType)
        {
            // if the type directly inherits from the base type, it will not overwrite any members
            if (type.BaseType?.Equals(baseType, SymbolEqualityComparer.Default) == true)
            {
                return false;
            }

            // otherwise, all derived types until the baseType must be abstract
            var next = type.BaseType;

            while (next?.Equals(baseType, SymbolEqualityComparer.Default) == false)
            {
                if (!next.IsAbstract)
                {
                    return true;
                }

                next = next.BaseType;
            }

            return false;
        }
    }
}
