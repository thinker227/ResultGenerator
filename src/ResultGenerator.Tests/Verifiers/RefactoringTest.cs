using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Model;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace ResultGenerator.Tests.Verifiers;

public class RefactoringTest<TRefactoring> : CSharpCodeRefactoringTest<TRefactoring, XUnitVerifier>
    where TRefactoring : CodeRefactoringProvider, new()
{
    public RefactoringTest() => SolutionTransforms.Add((solution, projectId) =>
    {
        var compilationOptions = solution.GetProject(projectId)!.CompilationOptions;
        compilationOptions = compilationOptions!.WithSpecificDiagnosticOptions(
            compilationOptions.SpecificDiagnosticOptions.SetItems(VerifierHelper.NullableWarnings));
        solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

        return solution;
    });

    protected override async Task RunImplAsync(CancellationToken cancellationToken)
    {
        Verify.NotEmpty($"{nameof(TestState)}.{nameof(SolutionState.Sources)}", TestState.Sources);

        var analyzers = GetDiagnosticAnalyzers().ToArray();
        var defaultDiagnostic = GetDefaultDiagnostic(analyzers);
        var supportedDiagnostics = analyzers
            .SelectMany(analyzer => analyzer.SupportedDiagnostics)
            .ToImmutableArray();
        var fixableDiagnostics = ImmutableArray<string>.Empty;

        var rawTestState = TestState.WithInheritedValuesApplied(null, fixableDiagnostics);
        var rawFixedState = FixedState.WithInheritedValuesApplied(rawTestState, fixableDiagnostics);

        var testState = rawTestState.WithProcessedMarkup(
            MarkupOptions,
            defaultDiagnostic,
            supportedDiagnostics,
            fixableDiagnostics,
            DefaultFilePath);
        var fixedState = rawFixedState.WithProcessedMarkup(
            MarkupOptions,
            defaultDiagnostic,
            supportedDiagnostics,
            fixableDiagnostics,
            DefaultFilePath);

        await VerifyDiagnosticsAsync(
                new EvaluatedProjectState(testState, ReferenceAssemblies),
                testState.AdditionalProjects.Values
                    .Select(additionalProject =>
                        new EvaluatedProjectState(additionalProject, ReferenceAssemblies))
                    .ToImmutableArray(),
                FilterTriggerSpanResults(testState.ExpectedDiagnostics).ToArray(),
                Verify.PushContext("Diagnostics of test state"),
                cancellationToken)
            .ConfigureAwait(false);

        if (CodeActionExpected(FixedState))
        {            
            await VerifyRefactoringAsync(
                    testState,
                    fixedState,
                    GetTriggerSpanResult(testState.ExpectedDiagnostics),
                    Verify,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        static IEnumerable<DiagnosticResult> FilterTriggerSpanResults(IEnumerable<DiagnosticResult> expected) =>
            expected.Where(result => result.Id != TriggerSpanDescriptor.Id);

        static DiagnosticResult GetTriggerSpanResult(IEnumerable<DiagnosticResult> expected)
        {
            var triggerSpan = null as DiagnosticResult?;
            foreach (var result in expected)
            {
                if (result.Id == TriggerSpanDescriptor.Id)
                {
                    Verify.Equal(null, triggerSpan, "Expected the test to only include a single trigger span for refactoring");
                    triggerSpan = result;
                }
            }

            Verify.True(triggerSpan.HasValue, "Expected the test to include a single trigger span for refactoring");
            return triggerSpan!.Value;
        }
    }
}
