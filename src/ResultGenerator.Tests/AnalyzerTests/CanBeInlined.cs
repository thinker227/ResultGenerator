using VerifyCS = ResultGenerator.Tests.Verifiers.AnalyzerVerifier<ResultGenerator.Analyzers.CanBeInlinedAnalyzer>;

namespace ResultGenerator.Tests.AnalyzerTests;

public class CanBeInlined
{
    [Fact]
    public Task Reports_CanBeInlined() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: {|RESGEN0005:A|}]
        public void Foo() {}
    }
    """);

    [Fact]
    public Task Reports_CanBeInlined_ForParameters() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: {|RESGEN0005:A(Str<string>, X<int>)|}]
        public void Foo() {}
    }
    """);
}
