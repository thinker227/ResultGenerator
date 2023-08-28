using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator.Models;

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
        if (GetTypeSymbolInfo(syntax, semanticModel) is not ITypeSymbol symbol) return null;

        var isNullableValueType = false;
        if (!symbol.IsReferenceType &&
            symbol is INamedTypeSymbol named &&
            named.IsGenericType &&
            named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
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

    public static ITypeSymbol? GetTypeSymbolInfo(
        TypeSyntax syntax,
        SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(syntax);
        var type = typeInfo.Type;

        // Filter out error types.
        return type is not IErrorTypeSymbol
            ? type
            : null;
    }
}
