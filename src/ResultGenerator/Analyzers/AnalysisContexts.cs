using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultGenerator.Analyzers;

public readonly struct ResultDeclarationAnalysisContext
{
    private readonly SyntaxNodeAnalysisContext ctx;

    public AttributeListSyntax Declaration => (AttributeListSyntax)ctx.Node;

    public IMethodSymbol DeclaringMethod => (IMethodSymbol)ctx.ContainingSymbol!;

    public SemanticModel SemanticModel => ctx.SemanticModel;

    public Compilation Compilation => ctx.Compilation;

    public AnalyzerOptions Options => ctx.Options;

    public bool IsGeneratedCode => ctx.IsGeneratedCode;

    public CancellationToken CancellationToken => ctx.CancellationToken;

    public ResultDeclarationAnalysisContext(SyntaxNodeAnalysisContext ctx) => this.ctx = ctx;

    public void ReportDiagnostic(Diagnostic diagnostic) => ctx.ReportDiagnostic(diagnostic);
}

public static class AnalysisContextExtensions
{
    public static void RegisterResultDeclarationAction(
        this SymbolStartAnalysisContext ctx,
        Action<ResultDeclarationAnalysisContext> analyze)
    {
        ctx.RegisterSyntaxNodeAction(syntaxNodeCtx =>
        {
            var declaration = (AttributeListSyntax)syntaxNodeCtx.Node;

            if (!declaration.IsResultDeclaration()) return;
            
            var resultDeclCtx = new ResultDeclarationAnalysisContext(syntaxNodeCtx);
            
            analyze(resultDeclCtx);
        }, SyntaxKind.AttributeList);
    }
}
