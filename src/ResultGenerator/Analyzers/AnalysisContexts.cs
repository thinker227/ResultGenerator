using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultGenerator.Analyzers;

internal static class AnalysisContextExtensions
{
#pragma warning disable RS1012

    public static void RegisterTypeProviderAction(
        this AnalysisContext ctx,
        Action<(
            WellKnownTypeProvider typeProvider,
            CompilationStartAnalysisContext compilationCtx)> analyze) =>
        ctx.RegisterCompilationStartAction(compilationCtx =>
        {
            var typeProvider = WellKnownTypeProvider.Create(compilationCtx.Compilation);
            if (typeProvider is null) return;

            analyze((typeProvider, compilationCtx));
        });
    
    public static void RegisterResultDeclaringMethodAction(
        this AnalysisContext ctx,
        Action<(
            ResultTypeDeclaringMethod declaringMethod,
            SymbolStartAnalysisContext symbolStartCtx)> analyze) =>
        ctx.RegisterTypeProviderAction(typeProvierCtx =>
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

                analyze((declaringMethod.Value, symbolStartCtx));
            }, SymbolKind.Method);
        });

    public static void RegisterResultDeclarationAction(
        this SymbolStartAnalysisContext ctx,
        Action<(
            AttributeListSyntax declaration,
            SyntaxNodeAnalysisContext syntaxNodeCtx)> analyze) => ctx.RegisterSyntaxNodeAction(
        syntaxNodeCtx =>
        {
            var declaration = (AttributeListSyntax)syntaxNodeCtx.Node;

            if (!declaration.IsResultDeclaration()) return;
        
            analyze((declaration, syntaxNodeCtx));
        }, SyntaxKind.AttributeList);
}
