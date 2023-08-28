using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ResultGenerator.CodeFixes;

// Code generated using https://roslynquoter.azurewebsites.net/.
internal static class SyntaxInator
{
    public static AttributeListSyntax DefaultResultDeclaration() =>
        AttributeList(
            SeparatedList<AttributeSyntax>(
                new SyntaxNodeOrToken[]{
                    Attribute(
                        IdentifierName("Ok")),
                    Token(SyntaxKind.CommaToken),
                    Attribute(
                        IdentifierName("Error"))}))
        .WithTarget(
            AttributeTargetSpecifier(
                Identifier("result")));
}
