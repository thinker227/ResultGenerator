using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator.Analysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Diagnostics.SpecifyResultDeclaration,
        Diagnostics.TooManyResultDeclarations);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.EnableConcurrentExecution();
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        ctx.RegisterCompilationStartAction(compilationCtx =>
        {
            var typeProvider = WellKnownTypeProvider.Create(compilationCtx.Compilation);
            if (typeProvider is null) return;

            compilationCtx.RegisterSymbolStartAction(symbolStartCtx =>
            {
                var method = (IMethodSymbol)symbolStartCtx.Symbol;

                if (method.DeclaringSyntaxReferences[0]!.GetSyntax() is not MethodDeclarationSyntax methodSyntax) return;

                var attribute = method.GetAttributes().FirstOrDefault(attr =>
                    attr.AttributeClass?.Equals(
                        typeProvider.ReturnsResultAttribute,
                        SymbolEqualityComparer.Default)
                        ?? false);

                if (attribute is null) return;

                var attributeSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!
                    .GetSyntax();
                
                var resultDeclarations = method.DeclaringSyntaxReferences
                    .Select(r => r.GetSyntax())
                    .OfType<MethodDeclarationSyntax>()
                    .SelectMany(node => node.AttributeLists)
                    .Where(attribute => attribute.Target?.Identifier.Text == "result")
                    .ToImmutableArray();

                AnalyzeResultDeclarations(symbolStartCtx, resultDeclarations, methodSyntax);
            }, SymbolKind.Method);
        });
    }

    private static void AnalyzeResultDeclarations(
        SymbolStartAnalysisContext ctx,
        ImmutableArray<AttributeListSyntax> resultDeclarations,
        MethodDeclarationSyntax methodSyntax)
    {
        ctx.RegisterSymbolEndAction(symbolEndCtx =>
        {
            if (resultDeclarations.Length == 0)
            {
                symbolEndCtx.ReportDiagnostic(
                    Diagnostic.Create(
                        Diagnostics.SpecifyResultDeclaration,
                        methodSyntax.Identifier.GetLocation()));
            }

            if (resultDeclarations.Length > 1)
            {
                foreach (var decl in resultDeclarations.Skip(1))
                {
                    symbolEndCtx.ReportDiagnostic(
                        Diagnostic.Create(
                            Diagnostics.TooManyResultDeclarations,
                            decl.GetLocation()));
                }
            }
        });

        foreach (var decl in resultDeclarations)
        {
            AnalyzeResultDeclaration(ctx, decl);
        }
    }

    private static void AnalyzeResultDeclaration(
        SymbolStartAnalysisContext ctx,
        AttributeListSyntax declaration)
    {
        
    }
}
