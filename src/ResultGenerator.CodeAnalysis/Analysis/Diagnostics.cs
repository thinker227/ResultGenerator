using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultGenerator.Analysis;

public static class Diagnostics
{
    public static DiagnosticDescriptor SpecifyResultDeclaration { get; } = new(
        "RESGEN0001",
        "Specify result declaration",
        "Specify a result declaration using [result: ...]",
        "ResultCorrectness",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TooManyResultDeclarations { get; } = new(
        "RESGEN0002",
        "Too many result declarations",
        "Only one result declaration may be specified per method",
        "ResultCorrectness",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidResultTypeName { get; } = new(
        "RESGEN0003",
        "Invalid result type name",
        "'{0}' is not a valid result type name",
        "ResultCorrectness",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Result type names have to be valid C# identifiers");

    public static DiagnosticDescriptor InvalidAttributeCtor { get; } = new(
        "RESGEN0004",
        "Unrecognized attribute constructor",
        "Attribute constructor should take either no parameters or one string parameter",
        "ResultCorrectness",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor CanBeInlined { get; } = new(
        "RESGEN0005",
        "Result type can be inlined",
        "Result type with only one variant can be removed and inlined into the return type of the method",
        "ResultQuality",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor BadValueSyntax { get; } = new(
        "RESGEN0006",
        "Invalid result value identifier",
        "Result value name has to be a single identifier",
        "ResultCorrectness",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor BadValueParamaterSyntax { get; } = new(
        "RESGEN0007",
        "Invalid value parameter syntax",
        "Value parameter has to be an identifier followed by angle brackets and a type",
        "ResultCorrectness",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TooManyValueParameterTypes { get; } = new(
        "RESGEN0008",
        "Too many value parameter types",
        "Value parameters can only specify a single type",
        "ResultCorrectness",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor UnknownType { get; } = new(
        "RESGEN0009",
        "Unknown type",
        "Unknown type '{0}'",
        "ResultCorrectness",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
