using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ResultGenerator;

/// <summary>
/// Helper for producing syntax nodes.
/// </summary>
/// <remarks>
/// Code generated using https://roslynquoter.azurewebsites.net/.
/// </remarks>
internal static class SyntaxInator
{
    public static AttributeListSyntax ReturnsResultAttribute() =>
        AttributeList(
            SingletonSeparatedList(
                Attribute(
                    IdentifierName("ReturnsResult"))));

    public static AttributeListSyntax DefaultResultDeclaration(TypeSyntax? okParameterType = null)
    {
        var okArgumentList = okParameterType is null
            ? null
            : AttributeArgumentList(
                SingletonSeparatedList(
                    AttributeArgument(
                        GenericName(
                            Identifier("Value"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList(
                                    okParameterType))))));

        return AttributeList(
            SeparatedList<AttributeSyntax>(
                new SyntaxNodeOrToken[]{
                    Attribute(
                        IdentifierName("Ok"))
                    .WithArgumentList(okArgumentList),
                    Token(SyntaxKind.CommaToken),
                    Attribute(
                        IdentifierName("Error"))}))
        .WithTarget(
            AttributeTargetSpecifier(
                Identifier("result")));
    }

    public static InvocationExpressionSyntax OkCall(string resultTypeName) =>
        InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(resultTypeName),
                IdentifierName("Ok")));

    public static ExpressionSyntax WrapExpressionWithOk(string resultTypeName, ExpressionSyntax? expression)
    {
        var invocation = OkCall(resultTypeName);

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
