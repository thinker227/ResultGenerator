using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultGenerator.Analyzers;

internal readonly struct ResultDeclaringMethodAnalysisContext
{
    public SymbolStartAnalysisContext SymbolContext { get; }
    
    public ResultTypeDeclaringMethod DeclaringMethod { get; }

    public IMethodSymbol Symbol => (IMethodSymbol)SymbolContext.Symbol;

    public Compilation Compilation => SymbolContext.Compilation;

    public bool IsGeneratedCode => SymbolContext.IsGeneratedCode;

    public AnalyzerOptions Options => SymbolContext.Options;

    public CancellationToken CancellationToken => SymbolContext.CancellationToken;

    public ResultDeclaringMethodAnalysisContext(
        SymbolStartAnalysisContext symbolContext,
        ResultTypeDeclaringMethod declaringMethod)
    {
        SymbolContext = symbolContext;
        DeclaringMethod = declaringMethod;
    }

    public void RegisterResultDeclarationAction(Action<ResultDeclarationAnalysisContext> analyze) =>
        SymbolContext.RegisterResultDeclarationAction(analyze);
}

internal readonly struct ResultDeclarationAnalysisContext
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

internal static class AnalysisContextExtensions
{
#pragma warning disable RS1012

    public static void RegisterTypeProviderAction(
        this AnalysisContext ctx,
        Action<(WellKnownTypeProvider typeProvider, CompilationStartAnalysisContext compilationCtx)> analyze) =>
        ctx.RegisterCompilationStartAction(compilationCtx =>
        {
            var typeProvider = WellKnownTypeProvider.Create(compilationCtx.Compilation);
            if (typeProvider is null) return;

            analyze((typeProvider, compilationCtx));
        });
    
    public static void RegisterResultDeclaringMethodAction(
        this AnalysisContext ctx,
        Action<ResultDeclaringMethodAnalysisContext> analyze) => ctx.RegisterTypeProviderAction(typeProvierCtx =>
    {
        typeProvierCtx.compilationCtx.RegisterSymbolStartAction(symbolStartCtx =>
        {
            var method = (IMethodSymbol)symbolStartCtx.Symbol;
                
            var declaringMethod = ResultTypeDeclaringMethod.Create(
                method,
                typeProvierCtx.typeProvider,
                null,
                checkPartialDeclarations: true);

            if (declaringMethod is null) return;

            var declaringMethodCtx = new ResultDeclaringMethodAnalysisContext(
                symbolStartCtx,
                declaringMethod.Value);

            analyze(declaringMethodCtx);
        }, SymbolKind.Method);
    });

    public static void RegisterResultDeclarationAction(
        this SymbolStartAnalysisContext ctx,
        Action<ResultDeclarationAnalysisContext> analyze) => ctx.RegisterSyntaxNodeAction(
        syntaxNodeCtx =>
        {
            var declaration = (AttributeListSyntax)syntaxNodeCtx.Node;

            if (!declaration.IsResultDeclaration()) return;
        
            var resultDeclCtx = new ResultDeclarationAnalysisContext(syntaxNodeCtx);
        
            analyze(resultDeclCtx);
        }, SyntaxKind.AttributeList);
}
