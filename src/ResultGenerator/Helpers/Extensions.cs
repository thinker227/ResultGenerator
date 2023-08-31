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

    /// <summary>
    /// Returns an <see cref="IMethodSymbol"/> for the other part of a partial method,
    /// or <see langword="null"/> if the method is not partial.
    /// </summary>
    /// <param name="method">The method to get the other partial part for.</param>
    public static IMethodSymbol? GetOtherPartialPart(this IMethodSymbol method) =>
        // Both parts are null        -> Method is not partial
        // Implementation is not null -> Method is partial definition
        // Definition is not null     -> Method is partial implementation
        method.PartialImplementationPart ?? method.PartialDefinitionPart;
}
