using System.Collections.Immutable;
using ResultGenerator.Helpers;
using ResultGenerator.Generation.Models;

namespace ResultGenerator.Generation;

internal sealed class TextWriter
{
    private readonly CodeStringBuilder builder;
    private readonly ResultTypeModel type;
    private readonly IReadOnlyDictionary<ResultValueModel, int> valueIndicies;

    private TextWriter(CodeStringBuilder builder, ResultTypeModel type)
    {
        this.builder = builder;
        this.type = type;

        valueIndicies = type.Values
            .Select((value, index) => (value, index))
            .ToDictionary(
                x => x.value,
                x => x.index + 1);
    }

    public static string Write(ResultTypeModel type)
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
        
        using System.Diagnostics.CodeAnalysis;
        using ResultGenerator.Internal;

        #nullable enable
        
        [ResultType]
        public readonly partial struct {GetTypeName(type)}
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
            builder.Append("\n\n");

            builder.Sections("\n", type.Values, WriteTryAsMethods);
            builder.Append("\n\n");

            WriteToStringMethod();
        }
    }

    private void WriteField(ResultValueModel value)
    {
        if (value.Parameters.IsEmpty)
        {
            builder.Append($"// Variant {value.Name} has no data.");
            return;
        }

        var paramsText = GetTupleOrRegularParameterTypeText(value.Parameters);

        builder.Append($"private readonly {paramsText} {GetFieldName(value)};");
    }

    private void WriteCtor()
    {
        builder.Append($"private {GetTypeName(type)}(int flag");

        foreach (var value in type.Values)
        {
            if (value.Parameters.Length == 0) continue;

            var typeText = GetTupleOrRegularParameterTypeText(value.Parameters);
            var name = GetParameterName(value);
            builder.Append($", {typeText} {name} = default!");
        }

        builder.AppendLine(")");

        using (builder.IndentedBlock("{", "}", true))
        {
            builder.AppendLine("this._flag = flag;");

            foreach (var value in type.Values)
            {
                if (value.Parameters.Length == 0) continue;

                var paramName = GetParameterName(value);
                var fieldName = GetFieldName(value);
                builder.AppendLine($"this.{fieldName} = {paramName};");
            }
        }

        builder.AppendLine();
    }

    private void WriteCreateMethod(ResultValueModel value)
    {
        var index = valueIndicies[value];
        
        builder.Append($"public static {GetTypeName(type)} {GetCreateMethodName(value)}");
        
        if (value.Parameters.IsEmpty)
        {
            builder.Append($"() => new({index});");
            return;
        }

        var valueParameterName = GetParameterName(value);
        var parameters = GetParameterListText(value.Parameters);
        var parameterNames = GetParameterNameListText(value.Parameters);

        builder.Append($"({parameters}) => new({index}, {valueParameterName}: ({parameterNames}));");
    }

    private void WriteIsProperties(ResultValueModel value)
    {
        var index = valueIndicies[value];

        builder.Append($"public bool {GetIsPropertyName(value)} => this._flag == {index};");
    }

    private void WriteTryAsMethods(ResultValueModel value)
    {
        if (value.Parameters.IsEmpty)
        {
            builder.Append($"// Variant {value.Name} has no data to try get.");
            return;
        }

        var index = valueIndicies[value];

        builder.Append($"public bool {GetTryAsMethodName(value)}(");

        builder.Sections(", ", value.Parameters, parameter =>
        {
            var typeText = GetTypeString(parameter.Type);
            var parameterName = GetParameterName(parameter);
            builder.Append($"[MaybeNullWhen(false)] out {typeText} {parameterName}");
        });

        builder.AppendLine("""
        )
        {
        """);
        builder.Indent();

        var variableTarget = GetTupleOrRegularParameterNameListText(value.Parameters);
        
        builder.AppendLine($"""
        {variableTarget} = this.{GetFieldName(value)};
        return this._flag == {index};
        """);

        builder.Unindent();
        builder.Append("""}""");
    }

    private void WriteToStringMethod()
    {
        builder.AppendLine("public override string? ToString()");

        using (builder.IndentedBlock("{", "}", true))
        {
            builder.Sections("\n", type.Values, value =>
            {
                var index = valueIndicies[value];
                builder.AppendLine($"if (this._flag == {index})");

                using (builder.IndentedBlock("{", "}", true))
                {
                    if (value.Parameters.IsEmpty)
                    {
                        builder.AppendLine($"""
                        return "{value.Name}";
                        """);
                        return;
                    }

                    builder.Append("var ");
                    builder.Append(GetTupleOrRegularParameterNameListText(value.Parameters));
                    builder.AppendLine($" = this.{GetFieldName(value)};");

                    // This goddamn insanity is why raw string literals exist.

                    builder.Append($$$""""
                    return $$"""
                    {{{value.Name}}} { 
                    """");

                    builder.Sections(", ", value.Parameters, parameter =>
                        builder.Append($$$"""
                        {{{parameter.Name}}} = {{{{{GetParameterName(parameter)}}}}}
                        """));

                    builder.AppendLine(""""
                     }
                    """;
                    """");
                }
            });
        
            builder.AppendLine();
            builder.AppendLine("return null;");
        }
    }

    private static string GetTypeName(ResultTypeModel type) =>
        $"@{type.Name}";

    private static string GetParameterName(ValueParameterModel parameter) =>
        $"@{CamelCase(parameter.Name)}";

    private static string GetParameterName(ResultValueModel value) =>
        $"@{CamelCase(value.Name)}";

    private static string GetFieldName(ResultValueModel value) =>
        $"_{CamelCase(value.Name)}Data";

    private static string GetCreateMethodName(ResultValueModel value) =>
        $"@{value.Name}";

    private static string GetIsPropertyName(ResultValueModel value) =>
        $"Is{value.Name}";

    private static string GetTryAsMethodName(ResultValueModel value) =>
        $"TryAs{value.Name}";

    private static string CamelCase(string str) =>
        $"{char.ToLowerInvariant(str[0])}{str[1..]}";

    private static string GetTypeString(ParameterTypeModel type) => type.IsNullable
        ? $"{type.FullyQualifiedName}?"
        : type.FullyQualifiedName;

    private static string GetTupleOrRegularParameterTypeText(EquatableArray<ValueParameterModel> parameters) => parameters switch
    {
        [var param] => GetTypeString(param.Type),
        _ => $"({GetTypeListText(parameters)})",
    };

    private static string GetParameterListText(EquatableArray<ValueParameterModel> parameters)
    {
        var texts = parameters
            .Select(p => $"{GetTypeString(p.Type)} {GetParameterName(p)}");
        return string.Join(", ", texts);
    }

    private static string GetParameterNameListText(EquatableArray<ValueParameterModel> parameters)
    {
        var texts = parameters
            .Select(GetParameterName);
        return string.Join(", ", texts);
    }

    private static string GetTupleOrRegularParameterNameListText(EquatableArray<ValueParameterModel> parameters) =>
        parameters is [var parameter]
            ? GetParameterName(parameter)
            : $"({GetParameterNameListText(parameters)})";

    private static string GetTypeListText(EquatableArray<ValueParameterModel> parameters)
    {
        var typesTexts = parameters.Select(p => GetTypeString(p.Type));
        return string.Join(", ", typesTexts);
    }
}
