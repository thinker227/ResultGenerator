using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ResultGenerator.CodeFixes;

// Code generated using https://roslynquoter.azurewebsites.net/.
internal static class SyntaxInator
{
    public static AttributeListSyntax DefaultResultDeclaration(SyntaxTriviaList leadingTrivia) =>
        AttributeList(
            SeparatedList<AttributeSyntax>(
                new SyntaxNodeOrToken[]{
                    Attribute(
                        IdentifierName("Ok")),
                    Token(
                        TriviaList(),
                        SyntaxKind.CommaToken,
                        TriviaList(
                            Space)),
                    Attribute(
                        IdentifierName("Error"))}))
        .WithOpenBracketToken(
            Token(
                leadingTrivia,
                SyntaxKind.OpenBracketToken,
                TriviaList()))
        .WithTarget(
            AttributeTargetSpecifier(
                Identifier("result"))
            .WithColonToken(
                Token(
                    TriviaList(),
                    SyntaxKind.ColonToken,
                    TriviaList(
                        Space))))
        .WithCloseBracketToken(
            Token(
                TriviaList(),
                SyntaxKind.CloseBracketToken,
                TriviaList(
                    LineFeed)));
}
