using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;

namespace ResultGenerator.Generation.Models;

internal readonly record struct ParameterTypeModel(
    string FullyQualifiedName,
    bool IsNullable,
    bool CanBeNull)
{
    public static ParameterTypeModel From(ITypeSymbol symbol, TypeSyntax syntax)
    {
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
