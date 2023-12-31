﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
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
        
        ctx.RegisterTypeProviderAction(typeProviderCtx =>
        {
            typeProviderCtx.compilationCtx.RegisterSymbolStartAction(symbolStartCtx =>
            {
                var method = (IMethodSymbol)symbolStartCtx.Symbol;
                
                // Check that the method is a partial method.
                if (method.GetOtherPartialPart() is not IMethodSymbol otherPartialPart) return;

                // Check whether the other partial part declares a result type.
                var declaringMethod = ResultTypeDeclaringMethod.Create(
                    otherPartialPart,
                    typeProviderCtx.typeProvider,
                    null,
                    checkPartialDeclarations: true);
                if (declaringMethod is null) return;

                symbolStartCtx.RegisterResultDeclarationAction(resultDeclCtx =>
                {
                    resultDeclCtx.syntaxNodeCtx.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.IgnoredResultDeclaration,
                        resultDeclCtx.declaration.GetLocation()));
                });
            }, SymbolKind.Method);
        });
    }
}
