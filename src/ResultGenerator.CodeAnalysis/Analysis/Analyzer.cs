using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;
using ResultGenerator.Models;

namespace ResultGenerator.Analysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Diagnostics.SpecifyResultDeclaration,
        Diagnostics.TooManyResultDeclarations,
        Diagnostics.InvalidResultTypeName,
        Diagnostics.InvalidAttributeCtor,
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

                if (method.DeclaringSyntaxReferences[0]!.GetSyntax() is not MethodDeclarationSyntax methodSyntax) return;

                var attribute = method.GetAttributes().FirstOrDefault(attr =>
                    attr.AttributeClass?.Equals(
                        typeProvider.ReturnsResultAttribute,
                        SymbolEqualityComparer.Default)
                        ?? false);

                if (attribute is null) return;

                AnalyzeAttribute(symbolStartCtx, attribute);

                var attributeSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!
                    .GetSyntax();
                
                var resultDeclarations = method.DeclaringSyntaxReferences
                    .Select(r => r.GetSyntax())
                    .OfType<MethodDeclarationSyntax>()
                    .SelectMany(node => node.AttributeLists)
                    .Where(attribute => attribute.Target?.Identifier.Text == "result")
                    .ToImmutableArray();

                AnalyzeResultDeclarations(symbolStartCtx, resultDeclarations, methodSyntax);
            }, SymbolKind.Method);
        });
    }

    private static void AnalyzeAttribute(
        SymbolStartAnalysisContext ctx,
        AttributeData attribute)
    {
        var syntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax();

        var ctorArgs = AttributeCtorArgs.Create(attribute);

        ctx.RegisterSymbolEndAction(symbolEndCtx =>
        {
            if (ctorArgs is not null) return;

            var location = syntax.ArgumentList!.GetLocation();

            symbolEndCtx.ReportDiagnostic(
                Diagnostic.Create(
                    Diagnostics.InvalidAttributeCtor,
                    location));
        });

        if (ctorArgs is not AttributeCtorArgs.WithTypeName { TypeName: var typeName }) return;

        ctx.RegisterSymbolEndAction(symbolEndCtx =>
        {
            if (SyntaxUtility.IsValidIdentifier(typeName)) return;

            var location = syntax.ArgumentList!.Arguments[0].Expression.GetLocation();

            symbolEndCtx.ReportDiagnostic(
                Diagnostic.Create(
                    Diagnostics.InvalidResultTypeName,
                    location,
                    typeName));
        });
    }

    private static void AnalyzeResultDeclarations(
        SymbolStartAnalysisContext ctx,
        ImmutableArray<AttributeListSyntax> resultDeclarations,
        MethodDeclarationSyntax methodSyntax)
    {
        ctx.RegisterSymbolEndAction(symbolEndCtx =>
        {
            if (resultDeclarations.Length == 0)
            {
                symbolEndCtx.ReportDiagnostic(
                    Diagnostic.Create(
                        Diagnostics.SpecifyResultDeclaration,
                        methodSyntax.Identifier.GetLocation()));
            }

            if (resultDeclarations.Length > 1)
            {
                foreach (var decl in resultDeclarations.Skip(1))
                {
                    symbolEndCtx.ReportDiagnostic(
                        Diagnostic.Create(
                            Diagnostics.TooManyResultDeclarations,
                            decl.GetLocation()));
                }
            }
        });

        foreach (var decl in resultDeclarations)
        {
            AnalyzeResultDeclaration(ctx, decl);
        }
    }

    private static void AnalyzeResultDeclaration(
        SymbolStartAnalysisContext ctx,
        AttributeListSyntax declaration)
    {
        ctx.RegisterSymbolEndAction(symbolEndCtx =>
        {
            if (declaration.Attributes.Count == 1)
            {
                var location = declaration.Attributes[0].GetLocation();

                symbolEndCtx.ReportDiagnostic(
                    Diagnostic.Create(
                        Diagnostics.CanBeInlined,
                        location));
            }
        });
    }
}
