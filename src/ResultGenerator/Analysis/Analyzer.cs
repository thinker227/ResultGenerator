using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;
using ResultGenerator.Models;
using Microsoft.CodeAnalysis.Text;
using Reporter = System.Action<Microsoft.CodeAnalysis.Diagnostic>;
using Microsoft.CodeAnalysis.CSharp;

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
        Diagnostics.UnknownType,
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
                // Only analyze ordinary method declarations.
                if (method.MethodKind is not MethodKind.Ordinary) return;

                // Get method syntax.
                // Even if the method is partial, the declaring
                // syntax references are never more than one.
                if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax) return;

                // Get attribute data.
                var attribute = method
                    .GetAttributeDataFor(typeProvider.ReturnsResultAttribute);
                if (attribute is null) return;

                // Get attribute syntax.
                // The application syntax reference *should* not be null.
                var attributeSyntax = (AttributeSyntax)
                    attribute.ApplicationSyntaxReference!.GetSyntax();

                {
                    var attributeMethodDeclaration = attributeSyntax
                        .FirstAncestorOrSelf<MethodDeclarationSyntax>()!;

                    // Check whether the method syntax the attribute was applied to is the
                    // same as the method syntax which the method symbol represents.
                    if (!methodSyntax.Equals(attributeMethodDeclaration)) return;
                }

                // Get result decalration(s).
                var resultDeclarations = methodSyntax
                    .GetResultDeclarations()
                    .ToImmutableArray();

                symbolStartCtx.RegisterSymbolEndAction(symbolEndCtx =>
                {
                    // Analyze method.
                    AnalyzeMethod(symbolEndCtx.ReportDiagnostic, method);

                    // Analyze attribute.
                    AnalyzeAttribute(
                        symbolEndCtx.ReportDiagnostic,
                        attribute,
                        attributeSyntax);

                    // Analyze result declaration list.
                    AnalyzeResultDeclarationList(
                        symbolEndCtx.ReportDiagnostic,
                        resultDeclarations,
                        methodSyntax);
                });

                // Analyze result declarations.
                symbolStartCtx.RegisterSyntaxNodeAction(syntaxNodeCtx =>
                {
                    var declaration = (AttributeListSyntax)
                        syntaxNodeCtx.Node;

                    if (!declaration.IsResultDeclaration()) return;

                    AnalyzeResultDeclaration(
                        syntaxNodeCtx.ReportDiagnostic,
                        syntaxNodeCtx.SemanticModel,
                        declaration);
                }, SyntaxKind.AttributeList);
            }, SymbolKind.Method);
        });
    }

    private void AnalyzeMethod(Reporter report, IMethodSymbol method)
    {
        // Get the other part of the partial declaration if there is one.
        IMethodSymbol otherPart;
        if (method.PartialDefinitionPart is IMethodSymbol declarationPart &&
            !declarationPart.Equals(method, SymbolEqualityComparer.Default))
        {
            otherPart = declarationPart;
        }
        else if (method.PartialImplementationPart is IMethodSymbol implementationPart &&
            !implementationPart.Equals(method, SymbolEqualityComparer.Default))
        {
            otherPart = implementationPart;
        }
        else return;

        var otherSyntax = (MethodDeclarationSyntax)
            otherPart.DeclaringSyntaxReferences[0].GetSyntax();
            
        var resultDeclarations = otherSyntax.GetResultDeclarations();

        foreach (var declaration in resultDeclarations)
        {
            var location = declaration.GetLocation();

            report(Diagnostic.Create(
                Diagnostics.IgnoredResultDeclaration,
                location));
        }
    }

    private static void AnalyzeAttribute(
        Reporter report,
        AttributeData attribute,
        AttributeSyntax syntax)
    {
        var ctorArgs = AttributeCtorArgs.Create(attribute);

        if (ctorArgs is null)
        {
            var location = syntax.ArgumentList!.GetLocation();

            report(Diagnostic.Create(
                Diagnostics.InvalidAttributeCtor,
                location));
        }

        if (ctorArgs is not AttributeCtorArgs.WithTypeName { TypeName: var typeName }) return;

        if (!Result.IsValidIdentifier(typeName))
        {
            var location = syntax.ArgumentList!.Arguments[0].Expression.GetLocation();

            report(Diagnostic.Create(
                Diagnostics.InvalidResultTypeName,
                location,
                typeName));
        }
    }

    private static void AnalyzeResultDeclarationList(
        Reporter report,
        ImmutableArray<AttributeListSyntax> resultDeclarations,
        MethodDeclarationSyntax methodSyntax)
    {
        if (resultDeclarations.Length == 0)
        {
            report(Diagnostic.Create(
                Diagnostics.SpecifyResultDeclaration,
                methodSyntax.Identifier.GetLocation()));
        }

        if (resultDeclarations.Length > 1)
        {
            foreach (var decl in resultDeclarations.Skip(1))
            {
                report(Diagnostic.Create(
                    Diagnostics.TooManyResultDeclarations,
                    decl.GetLocation()));
            }
        }
    }

    private static void AnalyzeResultDeclaration(
        Reporter report,
        SemanticModel semanticModel,
        AttributeListSyntax declaration)
    {
        if (declaration.Attributes.Count == 1)
        {
            var location = declaration.Attributes[0].GetLocation();

            report(Diagnostic.Create(
                Diagnostics.CanBeInlined,
                location));
        }

        foreach (var attribute in declaration.Attributes)
            AnalyzeResultValue(
                report,
                semanticModel,
                attribute);
    }

    private static void AnalyzeResultValue(
        Reporter report,
        SemanticModel semanticModel,
        AttributeSyntax value)
    {
        if (value.Name is not IdentifierNameSyntax)
        {
            var location = value.Name.GetLocation();

            report(Diagnostic.Create(
                Diagnostics.BadValueSyntax,
                location));
        }

        var parameters = value.ArgumentList?.Arguments
            .ToImmutableArray()
            ?? ImmutableArray<AttributeArgumentSyntax>.Empty;

        foreach (var parameter in parameters)
        {
            AnalyzeValueParameter(report, semanticModel, parameter);
        }
    }

    private static void AnalyzeValueParameter(
        Reporter report,
        SemanticModel semanticModel,
        AttributeArgumentSyntax parameter)
    {
        if (parameter.Expression is not GenericNameSyntax genericNameSyntax)
        {
            var location = parameter.Expression.GetLocation();

            report(Diagnostic.Create(
                Diagnostics.BadValueParamaterSyntax,
                location));

            return;
        }

        var parameterTypes = genericNameSyntax.TypeArgumentList.Arguments;

        if (parameterTypes.Count > 1)
        {
            var start = parameterTypes[1].Span.Start;
            var end = parameterTypes[^1].Span.End;
            var location = Location.Create(
                parameter.SyntaxTree,
                TextSpan.FromBounds(start, end));

            report(Diagnostic.Create(
                Diagnostics.TooManyValueParameterTypes,
                location));
        };

        foreach (var type in parameterTypes)
            AnalyzeParameterType(
                report,
                semanticModel,
                type);
    }

    private static void AnalyzeParameterType(
        Reporter report,
        SemanticModel semanticModel,
        TypeSyntax type)
    {
        if (Result.GetTypeSymbolInfo(type, semanticModel) is not null) return;

        var location = type.GetLocation();

        report(Diagnostic.Create(
            Diagnostics.UnknownType,
            location,
            type.GetText().ToString()));
    }
}
