using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ResultGenerator.Helpers;

namespace ResultGenerator.Generation.Models;

internal readonly record struct ResultValueModel(
    string Name,
    EquatableArray<ValueParameterModel> Parameters)
{
    public static ResultValueModel From(ResultValue value)
    {
        var name = value.Name;
        var parameters = value.Parameters
            .Select(ValueParameterModel.From)
            .ToImmutableArray()
            .AsEquatableArray();

        return new(name, parameters);
    }
}
