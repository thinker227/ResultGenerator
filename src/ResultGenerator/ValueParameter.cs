using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ResultGenerator;

/// <summary>
/// A parameter of a result value.
/// </summary>
/// <param name="Syntax">The syntax of the parameter.</param>
/// <param name="NameSyntax">The syntax of the name of the parameter.</param>
/// <param name="Name">The name of the parameter.</param>
/// <param name="TypeSyntax">The syntax of the type of the parameter.</param>
/// <param name="Type">The type of the parameter.</param>
internal readonly record struct ValueParameter(
    AttributeArgumentSyntax Syntax,
    GenericNameSyntax NameSyntax,
    string Name,
    TypeSyntax TypeSyntax,
    ITypeSymbol Type)
{
    /// <summary>
    /// Tries to create a <see cref="ValueParameter"/>.
    /// </summary>
    /// <param name="syntax">The original attribute argument syntax.</param>
    /// <param name="semanticModel">The semantic model for the operation.</param>
    /// <param name="diagnostics">A list of diagnostics.</param>
    /// <param name="parseInvalidDeclarations">Whether to parse invalid declarations for error checking.</param>
    /// <returns>The parsed value parameter,
    /// or <see langword="null"/> if it could not be parsed.</returns>
    public static ValueParameter? Create(
        AttributeArgumentSyntax syntax,
        SemanticModel semanticModel,
        ImmutableArray<Diagnostic>.Builder? diagnostics,
        bool parseInvalidDeclarations)
    {
        if (syntax.Expression is not GenericNameSyntax
        {
            Identifier.Text: var name,
            TypeArgumentList.Arguments: [var typeSyntax, ..] typeArguments
        } nameSyntax)
        {
            var location = syntax.Expression.GetLocation();

            diagnostics?.Add(Diagnostic.Create(
                Diagnostics.BadValueParamaterSyntax,
                location));
            
            return null;
        }

        if (typeArguments.Count > 1)
        {
            var start = typeArguments[1].Span.Start;
            var end = typeArguments[^1].Span.End;
            var location = Location.Create(
                syntax.SyntaxTree,
                TextSpan.FromBounds(start, end));

            diagnostics?.Add(Diagnostic.Create(
                Diagnostics.TooManyValueParameterTypes,
                location));

            if (parseInvalidDeclarations)
            {
                foreach (var arg in typeArguments)
                {
                    // Run error checking for invalid declarations.
                    _ = ParseType(
                        arg,
                        semanticModel,
                        diagnostics);
                }
            }
        }

        var type = ParseType(
            typeSyntax,
            semanticModel,
            diagnostics);

        if (type is null) return null;

        return new(
            syntax,
            nameSyntax,
            name,
            typeSyntax,
            type);
    }

    private static ITypeSymbol? ParseType(
        TypeSyntax syntax,
        SemanticModel semanticModel,
        ImmutableArray<Diagnostic>.Builder? diagnostics)
    {
        if (Result.GetTypeSymbolInfo(syntax, semanticModel) is not ITypeSymbol symbol)
        {
            var location = syntax.GetLocation();

            diagnostics?.Add(Diagnostic.Create(
                Diagnostics.UnknownType,
                location,
                syntax.GetText().ToString()));
            
            return null;
        }

        return symbol;
    }
}
