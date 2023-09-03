using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ResultGenerator.Refactorings;

internal static class SyntaxInator
{
    public static AttributeListSyntax ReturnsResultAttribute() =>
        AttributeList(
            SingletonSeparatedList(
                Attribute(
                    IdentifierName("ReturnsResult"))));

    public static AttributeListSyntax DefaultResultDeclarationWithParameter(TypeSyntax okParameterType) =>
        AttributeList(
                SeparatedList<AttributeSyntax>(
                    new SyntaxNodeOrToken[]{
                        Attribute(
                            IdentifierName("Ok"))
                        .WithArgumentList(
                            AttributeArgumentList(
                                SingletonSeparatedList(
                                    AttributeArgument(
                                        GenericName(
                                            Identifier("Value"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList(
                                                    okParameterType))))))),
                        Token(SyntaxKind.CommaToken),
                        Attribute(
                            IdentifierName("Error"))}))
            .WithTarget(
                AttributeTargetSpecifier(
                    Identifier("result")));

    public static ExpressionSyntax WrapExpressionWithOk(string resultTypeName, ExpressionSyntax? expression)
    {
        var invocation = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(resultTypeName),
                IdentifierName("Ok")));

        var argumentList = expression is not null
            ? SingletonSeparatedList(
                Argument(
                    expression))
            : default;

        return invocation.WithArgumentList(
            ArgumentList(
                argumentList));
    }
}
