using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator;

internal static class Result
{
    public static bool IsResultDeclaration(this AttributeListSyntax attribute) =>
        attribute.Target?.Identifier.Text == "result";

    public static IEnumerable<AttributeListSyntax> GetResultDeclarations(this MethodDeclarationSyntax syntax) =>
        syntax.AttributeLists .Where(IsResultDeclaration);

    public static ITypeSymbol? GetTypeSymbolInfo(
        TypeSyntax syntax,
        SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(syntax);
        var type = typeInfo.Type;

        // Filter out error types.
        return type is not IErrorTypeSymbol
            ? type
            : null;
    }

    public static string GetResultTypeName(IMethodSymbol method) =>
        method.Name + "Result"; 

    public static string GetResultTypeName(
        AttributeCtorArgs args,
        IMethodSymbol method) => args switch
    {
        AttributeCtorArgs.Empty => GetResultTypeName(method),
        AttributeCtorArgs.WithTypeName x => x.TypeName,
        _ => throw new InvalidOperationException(),
    };

    public static bool IsValidIdentifier(string identifier) =>
        identifier.Length >= 1 &&
        SyntaxFacts.IsIdentifierStartCharacter(identifier[0]) &&
        identifier.Skip(1).All(SyntaxFacts.IsIdentifierPartCharacter);
}
