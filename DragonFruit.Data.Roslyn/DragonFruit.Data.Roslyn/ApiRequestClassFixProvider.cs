using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace DragonFruit.Data.Roslyn;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ApiRequestClassFixProvider)), Shared]
public class ApiRequestClassFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(ApiRequestClassAnalyzer.DiagnosticId);

    public override FixAllProvider GetFixAllProvider() => null;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.Single();

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnosticNode = root?.FindNode(diagnostic.Location.SourceSpan);

        if (diagnosticNode is not ClassDeclarationSyntax declaration)
        {
            return;
        }

        context.RegisterCodeFix(CodeAction.Create(title: "Make class partial", createChangedSolution: c => MakeClassPartial(context.Document, declaration, c)), diagnostic);
    }

    private async Task<Solution> MakeClassPartial(Document document, MemberDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
    {
        var newClassDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

        return document.WithSyntaxRoot(newRoot).Project.Solution;
    }
}
