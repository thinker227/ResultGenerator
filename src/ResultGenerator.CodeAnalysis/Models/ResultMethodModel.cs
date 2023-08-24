using ResultGenerator.Helpers;

namespace ResultGenerator.Models;

internal readonly record struct ResultMethod(
    string Name,
    EquatableArray<ResultValue> Values);

internal readonly record struct ResultValue(
    string Name,
    EquatableArray<ValueParameter> Parameters);

internal readonly record struct ValueParameter(
    string Name,
    ParameterType Type);

internal readonly record struct ParameterType(
    string FullyQualifiedName,
    bool IsNullable);
