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
                var symbol = (IMethodSymbol)symbolStartCtx.Symbol;

                if (symbol.DeclaringSyntaxReferences[0]!.GetSyntax() is not MethodDeclarationSyntax symbolSyntax) return;

                var attribute = symbol.GetAttributes().FirstOrDefault(attr =>
                    attr.AttributeClass?.Equals(
                        typeProvider.ReturnsResultAttribute,
                        SymbolEqualityComparer.Default)
                        ?? false);

                if (attribute is null) return;

                var attributeSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!
                    .GetSyntax();
                
                var resultDeclarations = symbol.DeclaringSyntaxReferences
                    .Select(r => r.GetSyntax())
                    .OfType<MethodDeclarationSyntax>()
                    .SelectMany(node => node.AttributeLists)
                    .Where(attribute => attribute.Target?.Identifier.Text == "result")
                    .ToImmutableArray();

                symbolStartCtx.RegisterSymbolEndAction(symbolEndCtx =>
                {
                    if (resultDeclarations.Length == 0)
                    {
                        symbolEndCtx.ReportDiagnostic(
                            Diagnostic.Create(
                                Diagnostics.SpecifyResultDeclaration,
                                symbolSyntax.Identifier.GetLocation()));
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


            }, SymbolKind.Method);
        });
    }
}
