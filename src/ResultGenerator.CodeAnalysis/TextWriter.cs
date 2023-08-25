using System.Collections.Immutable;
using ResultGenerator.Helpers;
using ResultGenerator.Models;

namespace ResultGenerator;

internal sealed class TextWriter
{
    private readonly CodeStringBuilder builder;
    private readonly ResultType type;
    private readonly IReadOnlyDictionary<ResultValue, int> valueIndicies;

    private TextWriter(CodeStringBuilder builder, ResultType type)
    {
        this.builder = builder;
        this.type = type;

        valueIndicies = type.Values
            .Select((value, index) => (value, index))
            .ToDictionary(
                x => x.value,
                x => x.index + 1);
    }

    public static string Write(ResultType type)
    {
        var builder = new CodeStringBuilder();
        var writer = new TextWriter(builder, type);

        writer.Write();

        return builder.ToString();
    }

    private void Write()
    {
        // TODO: Write proper namespace + nested types.

        builder.AppendLine($"""
        /// <auto-generated/>
        
        #nullable enable
        
        public readonly struct {type.Name}
        """);
        
        using (builder.IndentedBlock("{", "}", true))
        {
            builder.AppendLine("""
            private readonly int _flag;

            """);

            builder.Sections("\n", type.Values, WriteField);
            builder.Append("\n\n");

            WriteCtor();

            builder.Sections("\n", type.Values, WriteCreateMethod);
            builder.Append("\n\n");

            builder.Sections("\n", type.Values, WriteIsProperties);
            builder.AppendLine();
        }
    }

    private void WriteField(ResultValue value)
    {
        if (value.Parameters.IsEmpty)
        {
            builder.Append($"// Variant {value.Name} has no data.");
            return;
        }

        var paramsText = GetTupleOrRegularParameterTypeText(value.Parameters);

        builder.Append($"private readonly {paramsText} {GetFieldName(value.Name)};");
    }

    private void WriteCtor()
    {
        builder.Append($"private {type.Name}(int flag");

        foreach (var value in type.Values)
        {
            if (value.Parameters.Length == 0) continue;

            var typeText = GetTupleOrRegularParameterTypeText(value.Parameters);
            var name = GetParameterName(value.Name);
            builder.Append($", {typeText} {name} = default!");
        }

        builder.AppendLine(")");

        using (builder.IndentedBlock("{", "}", true))
        {
            builder.AppendLine("this._flag = flag;");

            foreach (var value in type.Values)
            {
                if (value.Parameters.Length == 0) continue;

                var paramName = GetParameterName(value.Name);
                var fieldName = GetFieldName(value.Name);
                builder.AppendLine($"this.{fieldName} = {paramName};");
            }
        }

        builder.AppendLine();
    }

    private void WriteCreateMethod(ResultValue value)
    {
        var index = valueIndicies[value];
        
        builder.Append($"public static {type.Name} {value.Name}");
        
        if (value.Parameters.IsEmpty)
        {
            builder.Append($"() => new({index});");
            return;
        }

        var valueParameterName = GetParameterName(value.Name);
        var parameters = GetParameterListText(value.Parameters);
        var parameterNames = GetParameterNameListText(value.Parameters);

        builder.Append($"({parameters}) => new({index}, {valueParameterName}: ({parameterNames}));");
    }

    private void WriteIsProperties(ResultValue value)
    {
        var index = valueIndicies[value];

        builder.Append($"public bool Is{value.Name} => this._flag == {index};");
    }

    private static string GetParameterName(string name) =>
        $"{char.ToLowerInvariant(name[0])}{name[1..]}";

    private static string GetFieldName(string name) =>
        $"_{GetParameterName(name)}Data";

    private static string GetTypeString(ParameterType type) => type.IsNullable
        ? $"{type.FullyQualifiedName}?"
        : type.FullyQualifiedName;

    private static string GetTupleOrRegularParameterTypeText(EquatableArray<ValueParameter> parameters) => parameters switch
    {
        [var param] => GetTypeString(param.Type),
        _ => $"({GetTypeListText(parameters)})",
    };

    private static string GetParameterListText(EquatableArray<ValueParameter> parameters)
    {
        var texts = parameters
            .Select(p => $"{GetTypeString(p.Type)} {GetParameterName(p.Name)}");
        return string.Join(", ", texts);
    }

    private static string GetParameterNameListText(EquatableArray<ValueParameter> parameters)
    {
        var texts = parameters
            .Select(p => GetParameterName(p.Name));
        return string.Join(", ", texts);
    }

    private static string GetTypeListText(EquatableArray<ValueParameter> parameters)
    {
        var typesTexts = parameters.Select(p => GetTypeString(p.Type));
        return string.Join(", ", typesTexts);
    }
}
