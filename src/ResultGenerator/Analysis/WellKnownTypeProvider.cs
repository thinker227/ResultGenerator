using Microsoft.CodeAnalysis;

namespace ResultGenerator.Analysis;

internal record WellKnownTypeProvider(
    INamedTypeSymbol ReturnsResultAttribute)
{
    public static WellKnownTypeProvider? Create(Compilation compilation)
    {
        var returnsResultAttribute = compilation.GetTypeByMetadataName("ResultGenerator.ReturnsResultAttribute");

        if (returnsResultAttribute is null) return null;

        return new(returnsResultAttribute);
    }
}
