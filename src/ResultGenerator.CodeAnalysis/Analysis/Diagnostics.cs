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
}
