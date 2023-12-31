using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace ResultGenerator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ResultDeclarationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Diagnostics.SpecifyResultDeclaration,
        Diagnostics.TooManyResultDeclarations,
        Diagnostics.InvalidResultTypeName,
        Diagnostics.InvalidAttributeCtor,
        Diagnostics.BadValueSyntax,
        Diagnostics.BadValueParamaterSyntax,
        Diagnostics.TooManyValueParameterTypes,
        Diagnostics.UnknownType);

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.EnableConcurrentExecution();
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        ctx.RegisterTypeProviderAction(typeProviderCtx =>
        {
            typeProviderCtx.compilationCtx.RegisterSymbolStartAction(symbolStartCtx =>
            {
                var method = (IMethodSymbol)symbolStartCtx.Symbol;
                
                // Analyze declaring method.
                symbolStartCtx.RegisterSymbolEndAction(symbolEndCtx =>
                {
                    var declarationDiagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
                    ResultTypeDeclaringMethod.Create(
                        method,
                        typeProviderCtx.typeProvider,
                        declarationDiagnostics,
                        checkPartialDeclarations: true);

                    foreach (var diagnostic in declarationDiagnostics)
                        symbolEndCtx.ReportDiagnostic(diagnostic);
                });

                // Analyze result declarations.
                symbolStartCtx.RegisterSyntaxNodeAction(syntaxNodeCtx =>
                {
                    var declaration = (AttributeListSyntax)
                        syntaxNodeCtx.Node;

                    if (!declaration.IsResultDeclaration()) return;

                    var valueDiagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
                    ResultValue.ParseValues(
                        declaration,
                        syntaxNodeCtx.SemanticModel,
                        valueDiagnostics,
                        parseInvalidDeclarations: true);

                    foreach (var diagnostic in valueDiagnostics)
                        syntaxNodeCtx.ReportDiagnostic(diagnostic);
                }, SyntaxKind.AttributeList);
            }, SymbolKind.Method);
        });
    }
}
