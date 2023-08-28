namespace ResultGenerator;

/// <summary>
/// Specifies a method which returns a generated result type.
/// <br/>
/// Methods with this attribute should have a result declaration
/// which specifies the variants of the result,
/// specified as an attribute list with the <c>result:</c> target.
/// <br/>
/// Result variants may have parameters, specified as attribute arguments
/// with a single type argument which specifies the parameter type.
/// <code>
/// [ReturnsResult]
/// [result: Ok(Value&lt;Person&gt;), NotFound]
/// public GetPersonResult GetPerson(string name)
/// {
///     // ...
/// }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ReturnsResultAttribute : Attribute
{
    #pragma warning disable IDE0060

    // Tried using `string? typeName = null`, but that causes the compiler
    // to make `[ReturnsResult]` into `[ReturnsResult(null)]` which causes issues.

    /// <summary>
    /// Initializes a new <see cref="ReturnsResultAttribute"/> instance.
    /// </summary>
    public ReturnsResultAttribute() {}

    /// <summary>
    /// Initializes a new <see cref="ReturnsResultAttribute"/> instance.
    /// </summary>
    /// <param name="typeName">
    /// The name of the generated result type.
    /// The specified name has to be a valid C# identifier.
    /// <br/>
    /// If not specified, the generated type name will be
    /// the name of the target method + <c>Result</c>.
    /// </param>
    public ReturnsResultAttribute(string typeName) {}
}
