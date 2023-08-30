using VerifyCS = ResultGenerator.Tests.Verifiers.AnalyzerVerifier<ResultGenerator.Analysis.ThrowAnalyzer>;

namespace ResultGenerator.Tests.AnalyzerTests;

public class Throw
{
    [Fact]
    public Task ReportsThrowStatements() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public class C
    {
        public void M()
        {
            {|RESGEN0011:throw new Exception();|}
        }
    }
    """);

    [Fact]
    public Task ReportsThrowExpressions() => VerifyCS.VerifyAnalyzerAsync(Header + """
    public class C
    {
        public void M(bool b)
        {
            var x = b
                ? 1
                : {|RESGEN0011:throw new Exception()|};
            
            Console.WriteLine(x);
        }
    }
    """);
}
