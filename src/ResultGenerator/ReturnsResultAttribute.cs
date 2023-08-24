namespace ResultGenerator;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ReturnsResultAttribute : Attribute
{
    public ReturnsResultAttribute() {}
    
    public ReturnsResultAttribute(string typeName) {}
}
