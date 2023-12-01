using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DragonFruit.Data.Roslyn
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ApiRequestClassAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DA0001";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, "Partial class expected", "Class '{0}' should be marked as partial", "Design", DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeClassDecl, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClassDecl(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classDeclarationNode)
            {
                return;
            }

            // check if class inherits from apiRequestType using type checking
            var apiRequestType = context.Compilation.GetTypeByMetadataName(typeof(ApiRequest).FullName);
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationNode);

            if (apiRequestType == null || classSymbol == null || !InheritsFrom(classSymbol, apiRequestType))
            {
                return;
            }

            // check if class has partial keyword
            if (classDeclarationNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, classDeclarationNode.Identifier.GetLocation(), classDeclarationNode.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool InheritsFrom(INamedTypeSymbol symbol, ITypeSymbol type)
        {
            var baseType = symbol.BaseType;

            while (baseType != null)
            {
                if (type.Equals(baseType, SymbolEqualityComparer.Default))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }
    }
}
