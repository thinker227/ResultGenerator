namespace ResultGenerator.Models;

internal abstract record AttributeCtorArgs
{
    public sealed record Empty : AttributeCtorArgs;

    public sealed record WithTypeName(string TypeName) : AttributeCtorArgs;
}
