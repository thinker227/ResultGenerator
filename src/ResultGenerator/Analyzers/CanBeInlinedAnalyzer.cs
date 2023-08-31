using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultGenerator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CanBeInlinedAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Diagnostics.CanBeInlined);

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
                
                var declaringMethod = ResultTypeDeclaringMethod.Create(
                    method,
                    typeProvider,
                    null,
                    checkPartialDeclarations: true);

                if (declaringMethod is null) return;
                
                symbolStartCtx.RegisterResultDeclarationAction(resultDeclCtx =>
                {
                    var values = ResultValue.ParseValues(
                        resultDeclCtx.Declaration,
                        resultDeclCtx.SemanticModel,
                        null,
                        parseInvalidDeclarations: false);

                    if (values is not [var value]) return;

                    resultDeclCtx.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.CanBeInlined,
                        value.Syntax.GetLocation()));
                });
            }, SymbolKind.Method);
        });
    }
}
