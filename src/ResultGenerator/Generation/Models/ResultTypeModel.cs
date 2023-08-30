using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ResultGenerator.Helpers;

namespace ResultGenerator.Generation.Models;

internal readonly record struct ResultTypeModel(
    string Name,
    EquatableArray<ResultValueModel> Values)
{
    public static ResultTypeModel From(ResultType resultType)
    {
        var name = resultType.Name;
        var values = resultType.Values
            .Select(ResultValueModel.From)
            .ToImmutableArray()
            .AsEquatableArray();

        return new(name, values);
    }
}
