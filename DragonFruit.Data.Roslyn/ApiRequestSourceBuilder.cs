// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using System.Linq;
using System.Text;
using DragonFruit.Data.Requests;
using DragonFruit.Data.Roslyn.Entities;
using DragonFruit.Data.Roslyn.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using StrawberryShake.CodeGeneration;
using StrawberryShake.CodeGeneration.CSharp.Builders;

namespace DragonFruit.Data.Roslyn;

internal static class ApiRequestSourceBuilder
{
    private static readonly UsingDirectiveSyntax[] DefaultUsingStatements =
    [
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Net.Http")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DragonFruit.Data")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DragonFruit.Data.Requests"))
    ];

    public static SourceText Build(INamedTypeSymbol classSymbol, RequestSymbolMetadata metadata, RequestBodyType requestBodyType)
    {
        // classes are partial by default
        var classBuilder = new ClassBuilder()
                           .SetName(classSymbol.Name)
                           .AddImplements("global::DragonFruit.Data.Requests.IRequestBuilder");

        var serializerMethodParamBuilder = new ParameterBuilder()
                                           .SetType("global::DragonFruit.Data.Serializers.SerializerResolver")
                                           .SetName("serializerResolver");

        var methodBuilder = new MethodBuilder()
                            .SetName("BuildRequest")
                            .SetReturnType("global::System.Net.Http.HttpRequestMessage")
                            .SetAccessModifier(AccessModifier.Public)
                            .AddParameter(serializerMethodParamBuilder);

        // create a new UriBuilder called uriBuilder pulling in the uri from this.RequestPath
        var methodBodyBuilder = new CodeBlockBuilder()
                                .AddCode("var uriBuilder = new global::System.StringBuilder(this.RequestPath);")
                                .AddEmptyLine();

        // process queries
        if (metadata.Properties[ParameterType.Query].Any())
        {
            methodBodyBuilder.AddCode("var queryBuilder = new global::System.Text.StringBuilder();");
            methodBodyBuilder.AddEmptyLine();

            WriteUriQueryBuilder(methodBodyBuilder, metadata.Properties[ParameterType.Query].OfType<ParameterSymbolMetadata>(), "queryBuilder");

            methodBodyBuilder.AddCode(new IfBuilder().SetCondition("queryBuilder.Length > 0")
                                                     .AddCode("queryBuilder.Length--;")
                                                     .AddCode("uriBuilder.Append(\"?\").Append(queryBuilder);"));
            methodBodyBuilder.AddEmptyLine();
            methodBodyBuilder.AddEmptyLine();
        }

        // create request body
        methodBodyBuilder.AddCode("var request = new global::System.Net.Http.HttpRequestMessage(this.RequestMethod, uriBuilder.ToString());").AddEmptyLine();

        // process body content
        switch (requestBodyType)
        {
            case RequestBodyType.CustomBodyDirect:
                methodBodyBuilder.AddCode($"request.Content = {metadata.BodyProperty.Accessor};");
                break;

            case RequestBodyType.CustomBodySerialized:
                methodBodyBuilder.AddCode($"var requestContentData = {metadata.BodyProperty.Accessor}");
                methodBodyBuilder.AddCode(new IfBuilder().SetCondition("requestContentData != null")
                                                         .AddCode("request.Content = serializerResolver.Resolve(requestContentData.GetType(), global::DragonFruit.Data.Serializers.DataDirection.Out).Serialize(requestContentData);"));
                break;

            case RequestBodyType.FormUriEncoded:
                methodBodyBuilder.AddCode("var formContentBuilder = new global::System.Text.StringBuilder();");
                WriteUriQueryBuilder(methodBodyBuilder, metadata.Properties[ParameterType.Form].OfType<ParameterSymbolMetadata>(), "formContentBuilder");

                methodBodyBuilder.AddCode(new IfBuilder().SetCondition("formContentBuilder.Length > 0").AddCode("formContentBuilder.Length--;"));
                methodBodyBuilder.AddCode("request.Content = new global::System.Net.Http.StringContent(formContentBuilder.ToString(), global::System.Text.Encoding.UTF8, \"application/x-www-form-urlencoded\");");
                break;

            case RequestBodyType.FormMultipart:
                methodBodyBuilder.AddCode("var multipartContent = new global::System.Net.Http.MultipartFormDataContent();");

                int counter = 0;

                foreach (var symbol in metadata.Properties[ParameterType.Form].OfType<ParameterSymbolMetadata>())
                {
                    string variableName;

                    switch (symbol.Type)
                    {
                        case RequestSymbolType.ByteArray:
                        {
                            variableName = $"ba{++counter}";
                            methodBodyBuilder.AddCode($"var {variableName} = {symbol.Accessor};");
                            methodBodyBuilder.AddCode(new IfBuilder().SetCondition($"{variableName} != null").AddCode($"multipartContent.Add(new global::System.Net.Http.ByteArrayContent({variableName}), \"{symbol.ParameterName}\");"));
                            break;
                        }

                        case RequestSymbolType.Stream:
                        {
                            variableName = $"st{++counter}";
                            methodBodyBuilder.AddCode($"var {variableName} = {symbol.Accessor}");
                            methodBodyBuilder.AddCode(new IfBuilder().SetCondition($"{variableName} != null").AddCode($"multipartContent.Add(new global::System.Net.Http.StreamContent({variableName}), \"{symbol.ParameterName}\");"));
                            break;
                        }

                        case RequestSymbolType.Enum when symbol is EnumSymbolMetadata enumSymbol:
                        {
                            var line = $"global::System.Net.Http.StringContent(global::DragonFruit.Data.Converters.EnumConverter.GetEnumValue({symbol.Accessor}, global::DragonFruit.Data.Requests.EnumOption.{enumSymbol.EnumOption})), \"{symbol.ParameterName};\");";

                            if (symbol.Nullable)
                            {
                                methodBodyBuilder.AddCode(new IfBuilder().SetCondition($"{symbol.Accessor} != null").AddCode(line));
                            }
                            else
                            {
                                methodBodyBuilder.AddCode(line);
                            }

                            break;
                        }

                        case RequestSymbolType.Enumerable when symbol is EnumerableSymbolMetadata enumerableSymbol:
                        {
                            if (enumerableSymbol.IsByteArray)
                            {
                                goto case RequestSymbolType.ByteArray;
                            }

                            var enumerableBlock = new ForEachBuilder()
                                                  .SetLoopHeader($"var item in global::DragonFruit.Data.Converters.EnumerableConverter.GetPairs({symbol.Accessor}, global::DragonFruit.Data.Requests.EnumerableOption.{enumerableSymbol.EnumerableOption})")
                                                  .AddCode("multipartContent.Add(new global::System.Net.Http.StringContent(item.Value), item.Key);");

                            methodBodyBuilder.AddCode(new IfBuilder().SetCondition($"{symbol.Accessor} != null").AddCode(enumerableBlock));
                            break;
                        }

                        case RequestSymbolType.KeyValuePair:
                        {
                            var keyValuePairBlock = new ForEachBuilder()
                                                    .SetLoopHeader($"var item in (global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<string, string>>){symbol.Accessor}")
                                                    .AddCode("multipartContent.Add(new global::System.Net.Http.StringContent(item.Value), item.Key);");

                            methodBodyBuilder.AddCode(new IfBuilder().SetCondition($"{symbol.Accessor} != null").AddCode(keyValuePairBlock));
                            break;
                        }

                        default:
                        {
                            var line = $"multipartContent.Add(new global::System.Net.Http.StringContent({symbol.Accessor}), \"{symbol.ParameterName}\");";

                            if (symbol.Nullable)
                            {
                                methodBodyBuilder.AddCode(new IfBuilder().SetCondition($"{symbol.Accessor} != null").AddCode(line));
                            }
                            else
                            {
                                methodBodyBuilder.AddCode(line);
                            }

                            break;
                        }
                    }
                }

                methodBodyBuilder.AddCode("request.Content = multipartContent;");
                break;
        }

        // process headers
        if (metadata.Properties[ParameterType.Header].Any())
        {
            methodBodyBuilder.AddEmptyLine().AddEmptyLine();

            foreach (var symbol in metadata.Properties[ParameterType.Header].OfType<ParameterSymbolMetadata>())
            {
                var headerHandler = new IfBuilder().SetCondition($"{symbol.Accessor} != null");

                switch (symbol)
                {
                    case EnumerableSymbolMetadata enumerableSymbol:
                        headerHandler.AddCode(new ForEachBuilder().SetLoopHeader($"var kvp in global::DragonFruit.Data.Converters.EnumerableConverter.GetPairs({symbol.Accessor}, global::DragonFruit.Data.Requests.EnumerableOption.{enumerableSymbol.EnumerableOption}, \"{symbol.ParameterName}\", \"{enumerableSymbol.Separator}\")")
                                                                  .AddCode("request.Headers.Add(kvp.Key, kvp.Value);"));
                        break;

                    case KeyValuePairSymbolMetadata:
                        headerHandler.AddCode(new ForEachBuilder().SetLoopHeader($"var kvp in (global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<string, string>>){symbol.Accessor}")
                                                                  .AddCode("request.Headers.Add(kvp.Key, kvp.Value);"));
                        break;

                    case EnumSymbolMetadata enumSymbol:
                        headerHandler.AddCode($"request.Headers.Add(\"{symbol.ParameterName}\", global::DragonFruit.Data.Converters.EnumConverter.GetEnumValue({symbol.Accessor}, global::DragonFruit.Data.Requests.EnumOption.{enumSymbol.EnumOption}));");
                        break;

                    default:
                        headerHandler.AddCode($"request.Headers.Add(\"{symbol.ParameterName}\", {symbol.Accessor}.ToString());");
                        break;
                }

                methodBodyBuilder.AddCode(headerHandler).AddEmptyLine();
            }
        }

        methodBodyBuilder.AddCode("return request;");
        methodBuilder.AddCode(methodBodyBuilder);
        classBuilder.AddMethod(methodBuilder);

        var code = new StringBuilder(5000);

        using (var writer = new CodeWriter(code))
        {
            classBuilder.Build(writer);
        }

        // add class with generated code annotation
        var tree = CSharpSyntaxTree.ParseText(SourceText.From(code.ToString()));
        var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        var compilerGeneratedAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.CodeDom.Compiler.GeneratedCodeAttribute")).WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new[]
        {
            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("DragonFruit.Data"))),
            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("4.1.0")))
        })));

        classNode = classNode.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(compilerGeneratedAttribute)));

        return SyntaxFactory.CompilationUnit()
                            .AddUsings(DefaultUsingStatements)
                            .AddMembers(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(classSymbol.ContainingNamespace.ToDisplayString())))
                            .AddMembers(classNode)
                            .WithLeadingTrivia(SyntaxFactory.Comment("// <auto-generated>"))
                            .GetText();
    }

    private static void WriteUriQueryBuilder(CodeBlockBuilder builder, IEnumerable<ParameterSymbolMetadata> symbols, string builderName)
    {
        foreach (var querySymbol in symbols)
        {
            var codeBlock = querySymbol switch
            {
                EnumerableSymbolMetadata enumerableSymbol => $"global::DragonFruit.Data.Converters.EnumerableConverter.WriteEnumerable({builderName}, {querySymbol.Accessor}, global::DragonFruit.Data.Requests.EnumerableOption.{enumerableSymbol.EnumerableOption}, \"{querySymbol.ParameterName}\", \"{enumerableSymbol.Separator}\");",
                EnumSymbolMetadata enumSymbol => $"global::DragonFruit.Data.Converters.EnumConverter.WriteEnum({builderName}, {querySymbol.Accessor}, global::DragonFruit.Data.Requests.EnumOption.{enumSymbol.EnumOption}, \"{querySymbol.ParameterName}\");",
                KeyValuePairSymbolMetadata => $"global::DragonFruit.Data.Converters.KeyValuePairConverter.WriteKeyValuePairs({builderName}, {querySymbol.Accessor});",

                _ => $"{builderName}.AppendFormat(\"{{0}}={{1}}&\", \"{querySymbol.ParameterName}\", global::System.Uri.EscapeDataString({querySymbol.Accessor}.ToString()));"
            };

            if (querySymbol.Nullable)
            {
                builder.AddCode(new IfBuilder().SetCondition($"{querySymbol.Accessor} != null").AddCode(codeBlock)).AddEmptyLine();
            }
            else
            {
                builder.AddCode(codeBlock).AddEmptyLine();
            }
        }
    }
}
