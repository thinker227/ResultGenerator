using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;

namespace ResultGenerator;

/// <summary>
/// A value in a result type.
/// </summary>
/// <param name="Syntax">The syntax of the value.</param>
/// <param name="NameSyntax">The syntax of the name of the value.</param>
/// <param name="Name">The name of the value.</param>
/// <param name="Parameters">The parameters the value takes.</param>
internal readonly record struct ResultValue(
    AttributeSyntax Syntax,
    IdentifierNameSyntax NameSyntax,
    string Name,
    ImmutableArray<ValueParameter> Parameters)
{
    /// <summary>
    /// Parses an entire <see cref="AttributeListSyntax"/> into a list of values, omitting values which could not be parsed.
    /// </summary>
    /// <param name="syntax">The original attribute list syntax.</param>
    /// <param name="semanticModel">The semantic model for the operation.</param>
    /// <param name="parseInvalidDeclarations">Whether to parse invalid declarations for error checking.</param>
    /// <param name="errorCallbacks">The error callbacks to use for reporting errors.</param>
    /// <returns>A list of parsed values.</returns>
    public static ImmutableArray<ResultValue> ParseValues(
        AttributeListSyntax syntax,
        SemanticModel semanticModel,
        bool parseInvalidDeclarations,
        ResultTypeErrorCallbacks errorCallbacks) => syntax.Attributes
            .Select(attr => Create(
                attr,
                semanticModel,
                parseInvalidDeclarations,
                errorCallbacks))
            .NotNull()
            .ToImmutableArray();

    /// <summary>
    /// Tries to create a <see cref="ResultValue"/>.
    /// </summary>
    /// <param name="syntax">The original attribute syntax.</param>
    /// <param name="semanticModel">The semantic model for the operation.</param>
    /// <param name="parseInvalidDeclarations">Whether to parse invalid declarations for error checking.</param>
    /// <param name="errorCallbacks">The error callbacks to use for reporting errors.</param>
    /// <returns>The parsed result value,
    /// or <see langword="null"/> if it could not be parsed.</returns>
    public static ResultValue? Create(
        AttributeSyntax syntax,
        SemanticModel semanticModel,
        bool parseInvalidDeclarations,
        ResultTypeErrorCallbacks errorCallbacks)
    {
        // Names other than simple names are not supported.
        if (syntax.Name is not IdentifierNameSyntax
        {
            Identifier.Text: var name
        } nameSyntax)
        {
            errorCallbacks.InvalidValueNameSyntax?.Invoke(syntax, syntax.Name);
            return null;
        }

        var parameters = syntax.ArgumentList?.Arguments
            .Select(arg => ValueParameter.Create(
                arg,
                semanticModel,
                parseInvalidDeclarations,
                errorCallbacks))
            .NotNull()
            .ToImmutableArray()
            ?? ImmutableArray<ValueParameter>.Empty;

        return new(
            syntax,
            nameSyntax,
            name,
            parameters);
    }
}
