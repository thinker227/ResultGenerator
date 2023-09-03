using Microsoft.CodeAnalysis.CodeRefactorings;

namespace ResultGenerator.Tests.Verifiers;

public static partial class RefactoringVerifier<TRefactoring>
    where TRefactoring : CodeRefactoringProvider, new()
{
    public static async Task VerifyRefactoringAsync(string source, string fixedSource)
    {
        var test = new RefactoringTest<TRefactoring>()
        {
            TestCode = source.ReplaceLineEndings(),
            FixedCode = fixedSource.ReplaceLineEndings(),
        };

        await test.RunAsync();
    }
}
