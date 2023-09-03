using VerifyCS = ResultGenerator.Tests.Verifiers.RefactoringVerifier<ResultGenerator.Refactorings.ToResultRefactoring>;

namespace ResultGenerator.Tests.RefactoringTests;

public class ToResult
{
    [Fact]
    public Task Refactors_ExpressionBody() => VerifyCS.VerifyRefactoringAsync(Header + """
    public sealed class Class
    {
        public int F$$oo() => 0;
    }
    """, Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: Ok(Value<int>), Error]
        public FooResult Foo() => FooResult.Ok(0);
    }
    """);

    [Fact]
    public Task Refactors_VoidReturn() => VerifyCS.VerifyRefactoringAsync(Header + """
    public sealed class Class
    {
        public void F$$oo() {}
    }
    """, Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: Ok, Error]
        public FooResult Foo()
        {
            return FooResult.Ok();
        }
    }
    """);

    [Fact]
    public Task Refactors_TypeReturn() => VerifyCS.VerifyRefactoringAsync(Header + """
    public sealed class Class
    {
        public int F$$oo()
        {
            return 0;
        }
    }
    """, Header + """
    public sealed class Class
    {
        [ReturnsResult]
        [result: Ok(Value<int>), Error]
        public FooResult Foo()
        {
            return FooResult.Ok(0);
        }
    }
    """);
}
