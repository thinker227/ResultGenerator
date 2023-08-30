using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ResultGenerator.Analysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Diagnostics.ReplaceThrow);

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

                if (method.MethodKind is not MethodKind.Ordinary) return;
                
                symbolStartCtx.RegisterOperationAction(operationCtx =>
                {
                    var operation = (IThrowOperation)operationCtx.Operation;
                    
                    var location = operation.Syntax.GetLocation();

                    operationCtx.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.ReplaceThrow,
                        location));
                }, OperationKind.Throw);
            }, SymbolKind.Method);
        });
    }
}
