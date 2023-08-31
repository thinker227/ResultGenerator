using VerifyCS = ResultGenerator.Tests.Verifiers.AnalyzerVerifier<ResultGenerator.Analyzers.IgnoredDeclarationAnalyzer>;

namespace ResultGenerator.Tests.AnalyzerTests;

public class IgnoredDeclaration
{
    [Fact]
    public Task Reports_IgnoredResultDeclaration() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed partial class Class
    {
        [ReturnsResult]
        [result: A, B]
        public partial void Foo();
    }

    public sealed partial class Class
    {
        {|RESGEN0010:[result: C, D]|}
        public partial void Foo() {}
    } 
    """);
}
