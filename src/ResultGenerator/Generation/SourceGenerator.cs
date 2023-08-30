using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Generation.Models;

namespace ResultGenerator.Generation;

[Generator(LanguageNames.CSharp)]
public sealed class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext ctx)
    {
        var types = ctx.SyntaxProvider
            .ForAttributeWithMetadataName(
                "ResultGenerator.ReturnsResultAttribute",
                (node, _) => node is MethodDeclarationSyntax,
                (syntaxCtx, _) =>
                    ResultType.Create(syntaxCtx))
            .Where(model => model is not null)
            .Select((model, _) => model!.Value);

        ctx.RegisterSourceOutput(types, (sourceCtx, type) =>
            sourceCtx.AddSource(
                $"{type.Name}.g.cs",
                TextWriter.Write(type)));
    }
}
