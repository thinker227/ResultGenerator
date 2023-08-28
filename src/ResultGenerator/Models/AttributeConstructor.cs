using Microsoft.CodeAnalysis;

namespace ResultGenerator.Models;

internal abstract record AttributeCtorArgs
{
    public sealed record Empty : AttributeCtorArgs;

    public sealed record WithTypeName(string TypeName) : AttributeCtorArgs;

    public static AttributeCtorArgs? Create(AttributeData attribute) => attribute.ConstructorArguments switch
    {
        [] => new AttributeCtorArgs.Empty(),

        [{
            Kind: TypedConstantKind.Primitive,
            Value: string typeName,
        }] => new AttributeCtorArgs.WithTypeName(typeName),
        
        _ => null
    };
}
