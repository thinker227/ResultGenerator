using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator.Analysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Diagnostics.SpecifyResultList);

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

                var attribute = symbol.GetAttributes().FirstOrDefault(attr =>
                    attr.AttributeClass!.Equals(
                        typeProvider.ReturnsResultAttribute,
                        SymbolEqualityComparer.Default));

                if (attribute is null) return;

                var attributeSyntax = attribute.ApplicationSyntaxReference!;
                
                var resultDeclaration = symbol.DeclaringSyntaxReferences
                    .Select(r => r.GetSyntax())
                    .OfType<MethodDeclarationSyntax>()
                    .SelectMany(node => node.AttributeLists)
                    .Where(attribute => attribute.Target?.Identifier.Text == "result")
                    .ToImmutableArray();

                symbolStartCtx.RegisterSymbolEndAction(symbolEndCtx =>
                {
                    if (resultDeclaration.Length == 0)
                    {
                        symbolEndCtx.ReportDiagnostic(Diagnostic.Create(
                            Diagnostics.SpecifyResultList,
                            Location.Create(attributeSyntax.SyntaxTree, attributeSyntax.Span)));
                    }
                });
            }, SymbolKind.Method);
        });
    }
}
