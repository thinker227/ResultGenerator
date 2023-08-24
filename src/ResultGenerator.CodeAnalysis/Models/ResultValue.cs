using ResultGenerator.Helpers;

namespace ResultGenerator.Models;

internal readonly record struct ResultValue(
    string Name,
    EquatableArray<ValueParameter> Parameters);
