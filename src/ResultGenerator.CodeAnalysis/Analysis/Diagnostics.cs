using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultGenerator.Analysis;

public static class Diagnostics
{
    public static DiagnosticDescriptor SpecifyResultList { get; } = new(
        "RESGEN0001",
        "Specify result attribute",
        "Specify an attribute [result: ...]",
        "ResultCorrectness",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
            "{0} should specify an attribute [result: ...] " + 
            "because it has the [ReturnsResult] attribute applied, " +
            "otherwise the attribute will have no effect");
}
