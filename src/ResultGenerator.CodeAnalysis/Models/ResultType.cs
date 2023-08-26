using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;

namespace ResultGenerator.Models;

internal readonly record struct ResultType(
    string Name,
    EquatableArray<ResultValue> Values)
{
    public static ResultType? Create(
        GeneratorAttributeSyntaxContext ctx)
    {
        // Covered by SpecifyResultDeclaration and TooManyResultDeclarations diagnostics.
        if (ctx.Attributes is not [var attribute]) return null;
        var node = (MethodDeclarationSyntax)ctx.TargetNode;
        var symbol = (IMethodSymbol)ctx.TargetSymbol;

        if (AttributeCtorArgs.Create(attribute) is not AttributeCtorArgs args) return null;

        var name = GetResultTypeName(args, symbol);

        // Covered by InvalidResultTypeName diagnostic.
        if (!SyntaxUtility.IsValidIdentifier(name)) return null;

        var resultAttributeLists = node.AttributeLists
            .Where(attribute => attribute.Target?.Identifier.Text == "result")
            .ToImmutableArray();

        // Only one result specifier is allowed per method.
        if (resultAttributeLists is not [var resultAttributeList]) return null;

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

    private static string GetResultTypeName(
        AttributeCtorArgs args,
        IMethodSymbol method) => args switch
    {
        AttributeCtorArgs.Empty => method.Name + "Result",
        AttributeCtorArgs.WithTypeName x => x.TypeName,
        _ => throw new InvalidOperationException(),
    };
}
