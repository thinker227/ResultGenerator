using Microsoft.CodeAnalysis;

namespace ResultGenerator.Helpers;

public static class Extensions
{
    public static AttributeData? GetAttributeDataFor(
        this ISymbol symbol,
        INamedTypeSymbol attributeType,
        SymbolEqualityComparer? equalityComparer = null)
    {
        equalityComparer ??= SymbolEqualityComparer.Default;

        return symbol.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass?.Equals(attributeType, equalityComparer)
                ?? false);
    }

    public static bool IsNullableValueType(this INamedTypeSymbol symbol) =>
        symbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
}
