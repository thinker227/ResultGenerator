using VerifyCS = ResultGenerator.Tests.Verifiers.CSharpAnalyzerVerifier<ResultGenerator.Analysis.Analyzer>;

namespace ResultGenerator.Tests;

public class AnalyzerTests
{
    [Fact]
    public Task Reports_SpecifyResultDeclaration() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        public void {|RESGEN0001:Foo|}() =>
            throw new NotImplementedException();
    }
    """);

    [Fact]
    public Task Reports_TooManyResultDeclarations_ForOneDeclaration() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: A, B]
        {|RESGEN0002:[result: C, D]|}
        public void Foo() =>
            throw new NotImplementedException();
    }
    """);

    [Fact]
    public Task Reports_TooManyResultDeclarations_ForManyDeclarations() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: A, B]
        {|RESGEN0002:[result: C, D]|}
        {|RESGEN0002:[result: E, F]|}
        {|RESGEN0002:[result: G, H]|}
        public void Foo() =>
            throw new NotImplementedException();
    }
    """);

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("@")]
    [InlineData("?")]
    [InlineData("FooResult ")]
    [InlineData(" FooResult")]
    [InlineData(" FooResult ")]
    [InlineData("FooResult?")]
    public Task Reports_InvalidResultTypeName(string typeName) => VerifyCS.VerifyAnalyzerAsync(Header + $$"""
    public sealed class Class
    {
        [ReturnsResult({|RESGEN0003:"{{typeName}}"|})]
        [result: A, B]
        public void Foo() {}
    }
    """);
}
