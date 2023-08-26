using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;
using ResultGenerator.Models;
using Microsoft.CodeAnalysis.Text;

namespace ResultGenerator.Analysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Diagnostics.SpecifyResultDeclaration,
        Diagnostics.TooManyResultDeclarations,
        Diagnostics.InvalidResultTypeName,
        Diagnostics.InvalidAttributeCtor,
        Diagnostics.CanBeInlined,
        Diagnostics.BadValueSyntax,
        Diagnostics.BadValueParamaterSyntax,
        Diagnostics.TooManyValueParameterTypes,
        Diagnostics.UnknownType);

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

        foreach (var value in declaration.Attributes)
        {
            AnalyzeResultValue(ctx, value);
        }
    }

    private static void AnalyzeResultValue(
        SymbolStartAnalysisContext ctx,
        AttributeSyntax value)
    {
        ctx.RegisterSymbolEndAction(symbolEndCtx =>
        {
            if (value.Name is IdentifierNameSyntax) return;

            var location = value.Name.GetLocation();

            symbolEndCtx.ReportDiagnostic(
                Diagnostic.Create(
                    Diagnostics.BadValueSyntax,
                    location));
        });

        var parameters = value.ArgumentList?.Arguments
            .ToImmutableArray()
            ?? ImmutableArray<AttributeArgumentSyntax>.Empty;
        foreach (var parameter in parameters)
        {
            AnalyzeValueParameter(ctx, parameter);
        }
    }

    private static void AnalyzeValueParameter(
        SymbolStartAnalysisContext ctx,
        AttributeArgumentSyntax parameter)
    {
        ctx.RegisterSymbolEndAction(symbolEndCtx =>
        {
            if (parameter.Expression is not GenericNameSyntax)
            {
                var location = parameter.Expression.GetLocation();

                symbolEndCtx.ReportDiagnostic(
                    Diagnostic.Create(
                        Diagnostics.BadValueParamaterSyntax,
                        location));
            }
        });

        if (parameter.Expression is not GenericNameSyntax genericNameSyntax) return;

        var parameterTypes = genericNameSyntax.TypeArgumentList.Arguments;

        ctx.RegisterSymbolEndAction(symbolEndCtx =>
        {
            if (parameterTypes.Count <= 1) return;

            var start = parameterTypes[1].Span.Start;
            var end = parameterTypes[^1].Span.End;
            var location = Location.Create(
                parameter.SyntaxTree,
                TextSpan.FromBounds(start, end));

            symbolEndCtx.ReportDiagnostic(
                Diagnostic.Create(
                    Diagnostics.TooManyValueParameterTypes,
                    location));
        });

        foreach (var type in parameterTypes)
        {
            AnalyzeParameterType(ctx, type);
        }
    }

    private static void AnalyzeParameterType(
        SymbolStartAnalysisContext ctx,
        TypeSyntax type)
    {
        // TODO: Fix this
        var semanticModel = ctx.Compilation.GetSemanticModel(type.SyntaxTree);

        ctx.RegisterSymbolEndAction(symbolEndCtx =>
        {
            if (ParameterType.GetTypeSymbolInfo(type, semanticModel) is not null) return;

            var location = type.GetLocation();

            symbolEndCtx.ReportDiagnostic(
                Diagnostic.Create(
                    Diagnostics.UnknownType,
                    location,
                    type.GetText().ToString()));
        });
    }
}
