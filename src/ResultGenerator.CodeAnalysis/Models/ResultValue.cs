using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;

namespace ResultGenerator.Models;

internal readonly record struct ResultValue(
    string Name,
    EquatableArray<ValueParameter> Parameters)
{
    public static ResultValue? Create(
        AttributeSyntax syntax,
        SemanticModel semanticModel)
    {
        // Names other than simple names are not supported.
        if (syntax.Name is not IdentifierNameSyntax
        {
            Identifier.Text: var name
        })
            return null;

        var parameters = syntax.ArgumentList?.Arguments
            .Select(arg => ValueParameter.Create(arg, semanticModel))
            .NotNull()
            .ToImmutableArray()
            .AsEquatableArray()
            ?? ImmutableArray<ValueParameter>.Empty.AsEquatableArray();
        
        return new(name, parameters);
    }
}
