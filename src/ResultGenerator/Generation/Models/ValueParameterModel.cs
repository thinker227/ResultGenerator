namespace ResultGenerator.Generation.Models;

internal readonly record struct ValueParameterModel(
    string Name,
    ParameterTypeModel Type)
{
    public static ValueParameterModel From(ValueParameter parameter)
    {
        var name = parameter.Name;
        var type = ParameterTypeModel.From(parameter.Type, parameter.TypeSyntax);

        return new(name, type);
    }
}
