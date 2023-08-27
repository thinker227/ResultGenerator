using Microsoft.CodeAnalysis;

namespace ResultGenerator.Helpers;

public static class SymbolUtility
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
}
