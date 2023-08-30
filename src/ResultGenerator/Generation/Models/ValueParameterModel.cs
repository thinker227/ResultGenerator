using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator.Generation.Models;

internal readonly record struct ValueParameterModel(
    string Name,
    ParameterTypeModel Type)
{
    public static ValueParameterModel? Create(
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

        if (ParameterTypeModel.Create(typeSyntax, semanticModel) is not ParameterTypeModel type)
            return null;

        return new(name, type);
    }
}
