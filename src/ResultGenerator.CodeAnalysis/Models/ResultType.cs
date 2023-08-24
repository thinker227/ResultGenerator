using ResultGenerator.Helpers;

namespace ResultGenerator.Models;

internal readonly record struct ResultType(
    string Name,
    EquatableArray<ResultValue> Values);
