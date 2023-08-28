namespace ResultGenerator.Internal;

/// <summary>
/// Specifies a type which is a generated result type.
/// </summary>
/// <remarks>
/// This attribute is meant <i>purely</i> for usage by generated code
/// and should <b>not</b> be applied to types manually.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class ResultTypeAttribute : Attribute {}
