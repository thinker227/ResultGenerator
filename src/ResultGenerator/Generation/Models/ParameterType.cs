using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;

namespace ResultGenerator.Generation.Models;

internal readonly record struct ParameterType(
    string FullyQualifiedName,
    bool IsNullable,
    bool CanBeNull)
{
    public static ParameterType? Create(
        TypeSyntax syntax,
        SemanticModel semanticModel)
    {
        // Covered by diagnostic UnknownType.
        if (Result.GetTypeSymbolInfo(syntax, semanticModel) is not ITypeSymbol symbol) return null;

        var isNullableValueType = false;
        if (symbol is INamedTypeSymbol named &&
            named.IsNullableValueType())
        {
            isNullableValueType = true;
            symbol = named.TypeArguments[0];
        }

        var fullyQualifiedName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var isNullable =
            isNullableValueType ||
            syntax is NullableTypeSyntax;

        var canBeNull =
            isNullableValueType ||
            symbol.IsReferenceType;

        return new(
            fullyQualifiedName,
            isNullable,
            canBeNull);
    }
}
