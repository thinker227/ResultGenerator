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
            .ForAttributeWithMetadataName<ResultTypeModel?>(
                "ResultGenerator.ReturnsResultAttribute",
                (node, _) => node is MethodDeclarationSyntax,
                (syntaxCtx, _) =>
                {
                    var resultType = ResultTypeDeclaringMethod.Create(
                            (IMethodSymbol)syntaxCtx.TargetSymbol,
                            syntaxCtx.Attributes[0],
                            diagnostics: null,
                            checkPartialDeclarations: false)?
                        .ToResultType(
                            syntaxCtx.SemanticModel,
                            diagnostics: null,
                            parseInvalidDeclarations: false);

                    // ResultType.Create only returns null in case a fatal parse
                    // error occured, or if the method isn't a valid target at all.
                    if (resultType is null) return null;

                    return ResultTypeModel.From(resultType.Value);
                })
            .Where(model => model is not null)
            .Select((model, _) => model!.Value);

        ctx.RegisterSourceOutput(types, (sourceCtx, type) =>
            sourceCtx.AddSource(
                $"{type.Name}.g.cs",
                TextWriter.Write(type)));
    }
}
