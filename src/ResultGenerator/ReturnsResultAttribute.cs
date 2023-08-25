namespace ResultGenerator;

/// <summary>
/// Specifies a method which returns a generated result type.
/// <br/>
/// Methods with this attribute should have an attribute list with the <c>result</c> target,
/// the attribute names of which will specify the variants in the generated result type.
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
    /// <br/>
    /// If not specified, the generated type name will be
    /// the name of the target method + <c>Result</c>.
    /// </param>
    public ReturnsResultAttribute(string typeName) {}
}
