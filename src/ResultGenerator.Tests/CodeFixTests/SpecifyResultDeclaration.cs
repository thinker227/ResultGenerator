using VerifyCS = ResultGenerator.Tests.Verifiers.CodeFixVerifier<ResultGenerator.Analysis.ResultDeclarationAnalyzer, ResultGenerator.CodeFixes.SpecifyResultDeclarationCodeFix>;

namespace ResultGenerator.Tests.CodeFixTests;

public class SpecifyResultDeclaration
{
    [Fact]
    public Task FixesDiagnostic() => VerifyCS.VerifyCodeFixAsync(Header + """
    public sealed class Class
    {
        [ReturnsResult]
        public void {|RESGEN0001:Foo|}() {}
    }  
    """, Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: Ok, Error]
        public void Foo() {}
    }  
    """);
}
