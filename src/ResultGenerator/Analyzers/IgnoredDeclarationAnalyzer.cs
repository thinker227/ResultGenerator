using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ResultGenerator.Helpers;

namespace ResultGenerator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IgnoredDeclarationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Diagnostics.IgnoredResultDeclaration);

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
                
                // Check that the method is a partial method.
                if (method.GetOtherPartialPart() is not IMethodSymbol otherPartialPart) return;

                // Check whether the other partial part declares a result type.
                var declaringMethod = ResultTypeDeclaringMethod.Create(
                    otherPartialPart,
                    typeProvider,
                    null,
                    checkPartialDeclarations: true);
                if (declaringMethod is null) return;

                symbolStartCtx.RegisterSyntaxNodeAction(syntaxNodeCtx =>
                {
                    var declaration = (AttributeListSyntax)
                        syntaxNodeCtx.Node;

                    // Ignore if attribute list is not a result declaration.
                    if (!declaration.IsResultDeclaration()) return;
                    
                    syntaxNodeCtx.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.IgnoredResultDeclaration,
                        declaration.GetLocation()));
                }, SyntaxKind.AttributeList);
            }, SymbolKind.Method);
        });
    }
}
