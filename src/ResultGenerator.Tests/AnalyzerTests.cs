using VerifyCS = ResultGenerator.Tests.Verifiers.CSharpAnalyzerVerifier<ResultGenerator.Analysis.Analyzer>;

namespace ResultGenerator.Tests;

public class AnalyzerTests
{
    [Fact]
    public async Task ReportsSpecifyResultDeclaration()
    {
        await VerifyCS.VerifyAnalyzerAsync(Header + """
        public sealed class Class
        {
            [ReturnsResult]
            public void {|RESGEN0001:Foo|}() =>
                throw new NotImplementedException();
        }
        """);
    }
}
