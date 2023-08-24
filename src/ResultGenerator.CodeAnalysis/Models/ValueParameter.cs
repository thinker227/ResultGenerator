using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator.Models;

internal readonly record struct ValueParameter(
    string Name,
    ParameterType Type)
{
    public static ValueParameter? Create(
        AttributeArgumentSyntax argument,
        SemanticModel semanticModel)
    {
        if (argument.Expression is not GenericNameSyntax
        {
            Identifier.Text: var name,
            TypeArgumentList.Arguments: [var typeSyntax]
        })
            return null;

        if (ParameterType.Create(typeSyntax, semanticModel) is not ParameterType type)
            return null;

        return new(name, type);
    }
}
