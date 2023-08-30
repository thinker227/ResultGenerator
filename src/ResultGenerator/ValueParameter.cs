using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    /// <param name="parseInvalidDeclarations">Whether to parse invalid declarations for error checking.</param>
    /// <param name="errorCallbacks">The error callbacks to use for reporting errors.</param>
    /// <returns>The parsed value parameter,
    /// or <see langword="null"/> if it could not be parsed.</returns>
    public static ValueParameter? Create(
        AttributeArgumentSyntax syntax,
        SemanticModel semanticModel,
        bool parseInvalidDeclarations,
        ResultTypeErrorCallbacks errorCallbacks)
    {
        if (syntax.Expression is not GenericNameSyntax
        {
            Identifier.Text: var name,
            TypeArgumentList.Arguments: [var typeSyntax, ..] typeArguments
        } nameSyntax)
        {
            errorCallbacks.BadValueParameterSyntax?.Invoke(syntax, syntax.Expression);
            return null;
        }

        if (typeArguments.Count > 1)
        {
            errorCallbacks.TooManyValueParameterTypes?.Invoke(syntax, syntax.Expression, typeArguments);

            if (parseInvalidDeclarations)
            {
                foreach (var arg in typeArguments)
                {
                    // Run error checking for invalid declarations.
                    _ = ParseType(
                        arg,
                        semanticModel,
                        errorCallbacks);
                }
            }
        }

        var type = ParseType(
            typeSyntax,
            semanticModel,
            errorCallbacks);

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
        ResultTypeErrorCallbacks errorCallbacks)
    {
        if (Result.GetTypeSymbolInfo(syntax, semanticModel) is not ITypeSymbol symbol)
        {
            errorCallbacks.UnknownType?.Invoke(syntax);
            return null;
        }

        return symbol;
    }
}
