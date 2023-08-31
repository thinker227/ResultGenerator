using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
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
        
        ctx.RegisterResultDeclaringMethodAction(declaringMethodCtx =>
        {
            declaringMethodCtx.symbolStartCtx.RegisterResultDeclarationAction(resultDeclCtx =>
            {
                var values = ResultValue.ParseValues(
                    resultDeclCtx.declaration,
                    resultDeclCtx.syntaxNodeCtx.SemanticModel,
                    null,
                    parseInvalidDeclarations: false);

                if (values is not [var value]) return;

                resultDeclCtx.syntaxNodeCtx.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.CanBeInlined,
                    value.Syntax.GetLocation()));
            });
        });
    }
}
