using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;

namespace ResultGenerator.Generation.Models;

internal readonly record struct ResultValueModel(
    string Name,
    EquatableArray<ValueParameterModel> Parameters)
{
    public static ResultValueModel? Create(
        AttributeSyntax syntax,
        SemanticModel semanticModel)
    {
        // Names other than simple names are not supported.
        // Covered by diagnostic BadValueSyntax.
        if (syntax.Name is not IdentifierNameSyntax
        {
            Identifier.Text: var name
        })
            return null;

        var parameters = syntax.ArgumentList?.Arguments
            .Select(arg => ValueParameterModel.Create(arg, semanticModel))
            .NotNull()
            .ToImmutableArray()
            .AsEquatableArray()
            ?? ImmutableArray<ValueParameterModel>.Empty.AsEquatableArray();
        
        return new(name, parameters);
    }
}
