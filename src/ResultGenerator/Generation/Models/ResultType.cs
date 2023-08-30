using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;

namespace ResultGenerator.Generation.Models;

internal readonly record struct ResultType(
    string Name,
    EquatableArray<ResultValue> Values)
{
    public static ResultType? Create(
        GeneratorAttributeSyntaxContext ctx)
    {
        // Only one attribute should be allowed per method.
        // To improve error handling, if there are multiple attributes
        // then the first one should be used.
        if (ctx.Attributes is not [var attribute, ..]) return null;
        var node = (MethodDeclarationSyntax)ctx.TargetNode;
        var symbol = (IMethodSymbol)ctx.TargetSymbol;

        // Covered by diagnostic InvalidAttributeCtor.
        if (AttributeCtorArgs.Create(attribute) is not AttributeCtorArgs args) return null;

        var name = Result.GetResultTypeName(args, symbol);

        // Covered by diagnostic InvalidResultTypeName.
        if (!Result.IsValidIdentifier(name)) return null;

        var resultAttributeLists = node
            .GetResultDeclarations()
            .ToImmutableArray();

        // Only one result declaration is allowed per method.
        // To improve error handling, if there are multiple declarations
        // then the first one should be used.
        // Covered by diagnostics SpecifyResultDeclaration and TooManyResultDeclarations.
        if (resultAttributeLists is not [var resultAttributeList, ..]) return null;

        var values = resultAttributeList.Attributes
            .Select(attribute => ResultValue.Create(
                attribute,
                ctx.SemanticModel))
            .NotNull()
            .ToImmutableArray()
            .AsEquatableArray();

        return new(
            name,
            values);
    }
}
