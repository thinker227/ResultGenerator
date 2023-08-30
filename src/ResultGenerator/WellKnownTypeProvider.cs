using Microsoft.CodeAnalysis;

namespace ResultGenerator;

internal record WellKnownTypeProvider(
    INamedTypeSymbol ReturnsResultAttribute,
    INamedTypeSymbol ResultTypeAttribute)
{
    public static WellKnownTypeProvider? Create(Compilation compilation)
    {
        var returnsResultAttribute = compilation.GetTypeByMetadataName("ResultGenerator.ReturnsResultAttribute");
        var resultTypeAttribute = compilation.GetTypeByMetadataName("ResultGenerator.Internal.ResultTypeAttribute");

        if (returnsResultAttribute is null) return null;
        if (resultTypeAttribute is null) return null;

        return new(
            returnsResultAttribute,
            resultTypeAttribute);
    }
}
