namespace ResultGenerator.Models;

internal readonly record struct ParameterType(
    string FullyQualifiedName,
    bool IsNullable);
