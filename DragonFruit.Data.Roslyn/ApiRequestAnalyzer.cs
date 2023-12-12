using System.Collections.Immutable;
using System.Linq;
using DragonFruit.Data.Requests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DragonFruit.Data.Roslyn
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ApiRequestAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor PartialClassRule = new("DA0001", "Partial class expected", "Class '{0}' should be marked as partial", "Design", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor NestedClassNotAllowedRule = new("DA0002", "Nested class not allowed", "Class '{0}' should not be nested", "Design", DiagnosticSeverity.Error, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PropertyNoGetterRule = new("DA0003", "Property has no getter", "Property '{0}' has no accessible getter", "Design", DiagnosticSeverity.Warning, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor PropertyOrMethodNotInApiRequestRule = new("DA0004", "Property or Method not in ApiRequest", "'{0}' is not in an ApiRequest class", "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor PropertyOrMethodInaccessibleRule = new("DA0005", "Property or Method is inaccessible", "'{0}' is inaccessible. Properties should either be public, protected or protected internal.", "Design", DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MethodReturnsVoidRule = new("DA0006", "Method returns void", "Method '{0}' used to provide request values returns void", "Design", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor MethodHasParametersRule = new("DA0007", "Method has parameters", "Method '{0}' used to provide request values takes arguments", "Design", DiagnosticSeverity.Error, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(PartialClassRule, NestedClassNotAllowedRule,
            PropertyNoGetterRule, PropertyOrMethodNotInApiRequestRule, PropertyOrMethodInaccessibleRule,
            MethodReturnsVoidRule, MethodHasParametersRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeClassDecl, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDecl, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyDecl, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeClassDecl(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classDeclarationNode)
            {
                return;
            }

            // check if class inherits from apiRequestType using type checking
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationNode);
            var apiRequestType = context.Compilation.GetTypeByMetadataName(typeof(ApiRequest).FullName);

            if (classSymbol == null || !ApiRequestSourceGenerator.DerivesFrom(classSymbol, apiRequestType))
            {
                return;
            }

            // check if class has partial keyword
            if (!classDeclarationNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                context.ReportDiagnostic(Diagnostic.Create(PartialClassRule, classDeclarationNode.Identifier.GetLocation(), classDeclarationNode.Identifier.Text));
            }

            // check if class is nested
            if (classDeclarationNode.Parent is ClassDeclarationSyntax)
            {
                context.ReportDiagnostic(Diagnostic.Create(NestedClassNotAllowedRule, classDeclarationNode.Identifier.GetLocation(), classDeclarationNode.Identifier.Text));
            }
        }

        private void AnalyzeMethodDecl(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not MethodDeclarationSyntax methodDeclarationSyntax)
            {
                return;
            }

            var symbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
            var bodyAttributeSymbol = context.Compilation.GetTypeByMetadataName(typeof(RequestBodyAttribute).FullName);
            var parameterAttributeSymbol = context.Compilation.GetTypeByMetadataName(typeof(RequestParameterAttribute).FullName);

            // check if symbol attributes are either body or parameter
            if (symbol?.GetAttributes().Where(x => x.AttributeClass != null).Any(a => a.AttributeClass.Equals(bodyAttributeSymbol, SymbolEqualityComparer.Default) || a.AttributeClass.Equals(parameterAttributeSymbol, SymbolEqualityComparer.Default)) != true)
            {
                return;
            }

            // check for attributes (and null check symbol)
            if (!ApiRequestSourceGenerator.DerivesFrom(symbol.ContainingType, context.Compilation.GetTypeByMetadataName(typeof(ApiRequest).FullName)))
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertyOrMethodNotInApiRequestRule, methodDeclarationSyntax.Identifier.GetLocation(), methodDeclarationSyntax.Identifier.Text));
                return;
            }

            // check for method accessibility
            if (symbol.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Protected and not Accessibility.ProtectedOrInternal)
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertyOrMethodInaccessibleRule, methodDeclarationSyntax.Identifier.GetLocation(), methodDeclarationSyntax.Identifier.Text));
            }

            // check if method takes parameters
            if (methodDeclarationSyntax.ParameterList.Parameters.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(MethodHasParametersRule, methodDeclarationSyntax.Identifier.GetLocation(), methodDeclarationSyntax.Identifier.Text));
            }

            // check if method returns void
            if (methodDeclarationSyntax.ReturnType is PredefinedTypeSyntax { Keyword.ValueText: "void" })
            {
                context.ReportDiagnostic(Diagnostic.Create(MethodReturnsVoidRule, methodDeclarationSyntax.Identifier.GetLocation(), methodDeclarationSyntax.Identifier.Text));
            }
        }

        private void AnalyzePropertyDecl(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclarationSyntax)
            {
                return;
            }

            var symbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax);
            var bodyAttributeSymbol = context.Compilation.GetTypeByMetadataName(typeof(RequestBodyAttribute).FullName);
            var parameterAttributeSymbol = context.Compilation.GetTypeByMetadataName(typeof(RequestParameterAttribute).FullName);

            if (symbol?.GetAttributes().Any(a => a.AttributeClass.Equals(bodyAttributeSymbol, SymbolEqualityComparer.Default) || a.AttributeClass.Equals(parameterAttributeSymbol, SymbolEqualityComparer.Default)) != true)
            {
                return;
            }

            if (!ApiRequestSourceGenerator.DerivesFrom(symbol.ContainingType, context.Compilation.GetTypeByMetadataName(typeof(ApiRequest).FullName)))
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertyOrMethodNotInApiRequestRule, propertyDeclarationSyntax.Identifier.GetLocation(), propertyDeclarationSyntax.Identifier.Text));
                return;
            }

            if (symbol.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Protected and not Accessibility.ProtectedOrInternal)
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertyOrMethodInaccessibleRule, propertyDeclarationSyntax.Identifier.GetLocation(), propertyDeclarationSyntax.Identifier.Text));
            }

            // check for getter
            if (symbol.GetMethod == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertyNoGetterRule, propertyDeclarationSyntax.Identifier.GetLocation(), propertyDeclarationSyntax.Identifier.Text));
                return;
            }

            // check getter is public, protected or protected internal
            if (symbol.GetMethod.DeclaredAccessibility is Accessibility.Private or Accessibility.Internal)
            {
                context.ReportDiagnostic(Diagnostic.Create(PropertyOrMethodInaccessibleRule, propertyDeclarationSyntax.Identifier.GetLocation(), propertyDeclarationSyntax.Identifier.Text));
            }
        }
    }
}
