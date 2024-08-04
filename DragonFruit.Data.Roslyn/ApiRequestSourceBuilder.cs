// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Linq;
using System.Text;
using DragonFruit.Data.Requests;
using DragonFruit.Data.Roslyn.Entities;
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

    public static CompilationUnitSyntax Build(INamedTypeSymbol classSymbol, RequestSymbolMetadata metadata)
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
                                .AddCode("var uriBuilder = new global::System.UriBuilder(this.RequestPath);")
                                .AddEmptyLine();

        // process queries
        if (metadata.Properties[ParameterType.Query].Any())
        {
            methodBodyBuilder.AddCode("var queryBuilder = new global::System.Text.StringBuilder();");
            methodBodyBuilder.AddEmptyLine();

            foreach (var querySymbol in metadata.Properties[ParameterType.Query].OfType<PropertySymbolMetadata>())
            {
                var codeBlock = querySymbol switch
                {
                    EnumerableSymbolMetadata enumerableSymbol => $"global::DragonFruit.Data.Converters.EnumerableConverter.WriteEnumerable(queryBuilder, {querySymbol.Accessor}, global::DragonFruit.Data.Requests.EnumerableOption.{enumerableSymbol.EnumerableOption}, \"{querySymbol.ParameterName}\", \"{enumerableSymbol.Separator}\");",
                    EnumSymbolMetadata enumSymbol => $"global::DragonFruit.Data.Converters.EnumConverter.WriteEnum(queryBuilder, {querySymbol.Accessor}, global::DragonFruit.Data.Requests.EnumOption.{enumSymbol.EnumOption}, \"{querySymbol.ParameterName}\");",
                    KeyValuePairSymbolMetadata => $"global::DragonFruit.Data.Converters.KeyValuePairConverter.WriteKeyValuePairs(queryBuilder, {querySymbol.Accessor});",

                    _ => $"queryBuilder.AppendFormat(\"{{0}}={{1}}&\", \"{querySymbol.ParameterName}\", global::System.Uri.EscapeDataString({querySymbol.Accessor}.ToString()));"
                };

                if (querySymbol.Nullable)
                {
                    methodBodyBuilder.AddCode(new IfBuilder().SetCondition($"{querySymbol.Accessor} != null").AddCode(codeBlock)).AddEmptyLine();
                }
                else
                {
                    methodBodyBuilder.AddCode(codeBlock).AddEmptyLine();
                }
            }

            // remove trailing &, set query string
            methodBodyBuilder.AddCode(new IfBuilder().SetCondition("queryBuilder.Length > 0")
                                                     .AddCode("queryBuilder.Length--;")
                                                     .AddCode("uriBuilder.Query = queryBuilder.ToString();"));
            methodBodyBuilder.AddEmptyLine();
            methodBodyBuilder.AddEmptyLine();
        }

        // create request body
        methodBodyBuilder.AddCode("var request = new global::System.Net.Http.HttpRequestMessage(this.RequestMethod, uriBuilder.Uri);");

        methodBuilder.AddCode(methodBodyBuilder);
        classBuilder.AddMethod(methodBuilder);

        var code = new StringBuilder(5000);

        using (var writer = new CodeWriter(code))
        {
            classBuilder.Build(writer);
        }

        var compilationUnit = SyntaxFactory.CompilationUnit();

        compilationUnit = compilationUnit.AddUsings(DefaultUsingStatements);
        compilationUnit = compilationUnit.AddMembers(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(classSymbol.ContainingNamespace.ToDisplayString())));

        // merge sourceText and compilationUnit
        var sourceText = SourceText.From(code.ToString());
        var tree = CSharpSyntaxTree.ParseText(sourceText);

        return compilationUnit.AddMembers(tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>().ToArray());
    }
}
