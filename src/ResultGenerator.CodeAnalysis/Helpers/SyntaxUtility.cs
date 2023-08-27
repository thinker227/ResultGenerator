using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator.Helpers;

public static class SyntaxUtility
{
    public static bool IsValidIdentifier(string identifier) =>
        identifier.Length >= 1 &&
        SyntaxFacts.IsIdentifierStartCharacter(identifier[0]) &&
        identifier.Skip(1).All(SyntaxFacts.IsIdentifierPartCharacter);

    public static bool IsResultDeclaration(this AttributeListSyntax attribute) =>
        attribute.Target?.Identifier.Text == "result";

    public static IEnumerable<AttributeListSyntax> GetResultDeclarations(this MethodDeclarationSyntax syntax) =>
        syntax.AttributeLists .Where(IsResultDeclaration);

    public static TNode? GetAncestorNode<TNode>(this SyntaxNode node)
        where TNode : SyntaxNode
    {
        var current = node.Parent;

        while (current is not null)
        {
            if (current is TNode x) return x;

            current = current.Parent;
        }

        return null;
    }
}
