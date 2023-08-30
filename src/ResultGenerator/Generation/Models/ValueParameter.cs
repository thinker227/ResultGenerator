using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator.Generation.Models;

internal readonly record struct ValueParameter(
    string Name,
    ParameterType Type)
{
    public static ValueParameter? Create(
        AttributeArgumentSyntax argument,
        SemanticModel semanticModel)
    {
        // Covered by diagnostic BadValueParamaterSyntax.
        if (argument.Expression is not GenericNameSyntax
        {
            Identifier.Text: var name,
            // Covered by diagnostic TooManyValueParameterTypes.
            TypeArgumentList.Arguments: [var typeSyntax]
        })
            return null;

        if (ParameterType.Create(typeSyntax, semanticModel) is not ParameterType type)
            return null;

        return new(name, type);
    }
}
