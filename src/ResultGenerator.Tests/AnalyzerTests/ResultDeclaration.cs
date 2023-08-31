using VerifyCS = ResultGenerator.Tests.Verifiers.AnalyzerVerifier<ResultGenerator.Analyzers.ResultDeclarationAnalyzer>;

namespace ResultGenerator.Tests.AnalyzerTests;

public class ResultDeclaration
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
    public Task Reports_BadValueSyntax_ForSimpleValues() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: {|RESGEN0006:X.Y|}, Z]
        public void Foo() {}
    }
    """);

    [Fact]
    public Task Reports_BadValueSyntax_ForValuesWithParameters() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: {|RESGEN0006:X.Y|}(Str<string>), Z]
        public void Foo() {}
    }
    """);

    [Fact]
    public Task Reports_BadValueParamaterSyntax() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: A({|RESGEN0007:X.Y|}), B]
        public void Foo() {}
    }
    """);

    [Fact]
    public Task Reports_TooManyValueParameterTypes_ForTwoTypes() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: A(X<int, {|RESGEN0008:string|}>), B]
        public void Foo() {}
    } 
    """);

    [Fact]
    public Task Reports_TooManyValueParameterTypes_ForManyTypes() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: A(X<int, {|RESGEN0008:string, double, bool|}>), B]
        public void Foo() {}
    } 
    """);

    [Fact]
    public Task Reports_UnknownType() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: A(X<{|RESGEN0009:Foo|}>), B]
        public void Foo() {}
    } 
    """);

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
