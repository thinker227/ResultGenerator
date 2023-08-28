using Microsoft.CodeAnalysis.Testing;
using VerifyCS = ResultGenerator.Tests.Verifiers.CSharpIncrementalGeneratorVerifier<ResultGenerator.SourceGenerator>;

namespace ResultGenerator.Tests;

public class GeneratorTests
{
    [Fact]
    public async Task GeneratesResultFromInferredName()
    {
        var code = Header + """
        public sealed class PersonService
        {
            [ReturnsResult]
            [result: Ok, NotFound]
            public void GetPerson() =>
                throw new NotImplementedException();
        }
        """;

        var expected = """
        /// <auto-generated/>

        using System.Diagnostics.CodeAnalysis;
        using ResultGenerator.Internal;

        #nullable enable

        [ResultType]
        public readonly struct @GetPersonResult
        {
            private readonly int _flag;
            
            // Variant Ok has no data.
            // Variant NotFound has no data.
            
            private @GetPersonResult(int flag)
            {
                this._flag = flag;
            }

            public static @GetPersonResult Ok() => new(1);
            public static @GetPersonResult NotFound() => new(2);
            
            public bool IsOk => this._flag == 1;
            public bool IsNotFound => this._flag == 2;
            
            // Variant Ok has no data to try get.
            // Variant NotFound has no data to try get.
        }

        """;

        await VerifyCS.VerifyGeneratorAsync(
            code,
            ("GetPersonResult.g.cs", expected));
    }

    [Fact]
    public async Task GeneratesResultFromExplicitName()
    {
        var code = Header + """
        public sealed class PersonService
        {
            [ReturnsResult("GetPersonRes")]
            [result: Ok, NotFound]
            public void GetPerson() =>
                throw new NotImplementedException();
        }
        """;

        var expected = """
        /// <auto-generated/>

        using System.Diagnostics.CodeAnalysis;
        using ResultGenerator.Internal;

        #nullable enable

        [ResultType]
        public readonly struct @GetPersonRes
        {
            private readonly int _flag;
            
            // Variant Ok has no data.
            // Variant NotFound has no data.
            
            private @GetPersonRes(int flag)
            {
                this._flag = flag;
            }

            public static @GetPersonRes Ok() => new(1);
            public static @GetPersonRes NotFound() => new(2);
            
            public bool IsOk => this._flag == 1;
            public bool IsNotFound => this._flag == 2;
            
            // Variant Ok has no data to try get.
            // Variant NotFound has no data to try get.
        }

        """;

        await VerifyCS.VerifyGeneratorAsync(
            code,
            ("GetPersonRes.g.cs", expected));
    }

    [Fact]
    public async Task GeneratesSingleParameters()
    {
        var code = Header + """
        public sealed class Class
        {
            [ReturnsResult]
            [result: A(Str<string>), B]
            public void Foo() =>
                throw new NotImplementedException();
        }
        """;

        var expected = """
        /// <auto-generated/>

        using System.Diagnostics.CodeAnalysis;
        using ResultGenerator.Internal;

        #nullable enable

        [ResultType]
        public readonly struct @FooResult
        {
            private readonly int _flag;
            
            private readonly string _aData;
            // Variant B has no data.
            
            private @FooResult(int flag, string a = default!)
            {
                this._flag = flag;
                this._aData = a;
            }

            public static @FooResult A(string str) => new(1, a: (str));
            public static @FooResult B() => new(2);
            
            public bool IsA => this._flag == 1;
            public bool IsB => this._flag == 2;
            
            public bool TryAsA([MaybeNullWhen(false)] out string str)
            {
                str = this._aData;
                return this._flag == 1;
            }
            // Variant B has no data to try get.
        }

        """;
        
        await VerifyCS.VerifyGeneratorAsync(
            code,
            ("FooResult.g.cs", expected));
    }

    [Fact]
    public async Task GeneratesMultipleParameters()
    {
        var code = Header + """
        public sealed class Class
        {
            [ReturnsResult]
            [result: A(Str<string>, X<int>, B<bool>), B]
            public void Foo() =>
                throw new NotImplementedException();
        }
        """;

        var expected = """
        /// <auto-generated/>

        using System.Diagnostics.CodeAnalysis;
        using ResultGenerator.Internal;

        #nullable enable

        [ResultType]
        public readonly struct @FooResult
        {
            private readonly int _flag;
            
            private readonly (string, int, bool) _aData;
            // Variant B has no data.
            
            private @FooResult(int flag, (string, int, bool) a = default!)
            {
                this._flag = flag;
                this._aData = a;
            }

            public static @FooResult A(string str, int x, bool b) => new(1, a: (str, x, b));
            public static @FooResult B() => new(2);
            
            public bool IsA => this._flag == 1;
            public bool IsB => this._flag == 2;
            
            public bool TryAsA([MaybeNullWhen(false)] out string str, [MaybeNullWhen(false)] out int x, [MaybeNullWhen(false)] out bool b)
            {
                (str, x, b) = this._aData;
                return this._flag == 1;
            }
            // Variant B has no data to try get.
        }

        """;
        
        await VerifyCS.VerifyGeneratorAsync(
            code,
            ("FooResult.g.cs", expected));
    }

    [Fact]
    public async Task GeneratesKeywordTypeName()
    {
        var code = Header + """
        public sealed class Class
        {
            [ReturnsResult("class")]
            [result: A, B]
            public void Foo() =>
                throw new NotImplementedException();
        }
        """;

        var expected = """
        /// <auto-generated/>

        using System.Diagnostics.CodeAnalysis;
        using ResultGenerator.Internal;

        #nullable enable

        [ResultType]
        public readonly struct @class
        {
            private readonly int _flag;
            
            // Variant A has no data.
            // Variant B has no data.
            
            private @class(int flag)
            {
                this._flag = flag;
            }

            public static @class A() => new(1);
            public static @class B() => new(2);
            
            public bool IsA => this._flag == 1;
            public bool IsB => this._flag == 2;
            
            // Variant A has no data to try get.
            // Variant B has no data to try get.
        }

        """;
        
        await VerifyCS.VerifyGeneratorAsync(
            code,
            ("class.g.cs", expected));
    }
}
