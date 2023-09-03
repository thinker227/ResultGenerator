using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Operations;

namespace ResultGenerator.Refactorings;

[ExportCodeRefactoringProvider(LanguageNames.CSharp)]
public sealed class ToResultRefactoring : CodeRefactoringProvider
{
    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext ctx)
    {
        var document = ctx.Document;

        var root = await document.GetSyntaxRootAsync();

        if (root?.FindNode(ctx.Span) is not MethodDeclarationSyntax methodDeclaration) return;

        var semanticModel = await document.GetSemanticModelAsync(ctx.CancellationToken);
        var compilation = semanticModel!.Compilation;
        var typeProvider = WellKnownTypeProvider.Create(compilation);
        if (typeProvider is null) return;

        if (semanticModel.GetDeclaredSymbol(methodDeclaration, ctx.CancellationToken) is not IMethodSymbol method) return;

        // Refactoring should not be available for methods which already declare a result type.
        if (ResultTypeDeclaringMethod.Create(
                method,
                typeProvider,
                null,
                checkPartialDeclarations: true)
            is not null) return;
        
        ctx.RegisterRefactoring(CodeAction.Create(
            "Change return type to result",
            cts => Execute(
                cts,
                document,
                semanticModel,
                method,
                methodDeclaration)));
    }

    private static async Task<Document> Execute(
        CancellationToken cts,
        Document document,
        SemanticModel semanticModel,
        IMethodSymbol methodSymbol,
        MethodDeclarationSyntax methodDeclaration)
    {
        var name = Result.GetResultTypeName(methodSymbol);
        var returnTypeSymbol = methodSymbol.ReturnType;
        var returnType = methodDeclaration.ReturnType;

        // Add attributes.
        var returnsResultAttribute = SyntaxInator.ReturnsResultAttribute()
            .WithAdditionalAnnotations(Formatter.Annotation);

        var resultDeclaration = SyntaxInator.DefaultResultDeclaration(
                methodSymbol.ReturnsVoid
                    ? null
                    : returnType)
            .WithAdditionalAnnotations(Formatter.Annotation);

        // Change return type.
        var newReturnType = SyntaxFactory.IdentifierName(name)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newExpressionBody = null as ArrowExpressionClauseSyntax;
        var newBody = null as BlockSyntax;

        // Update expression body.
        if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax {
            Expression: var expressiobBodyExpr
        } expressionBody)
        {
            var wrappedExpr = SyntaxInator.WrapExpressionWithOk(name, expressiobBodyExpr);
            newExpressionBody = expressionBody.WithExpression(wrappedExpr);
        }
        // Update block body.
        else if (methodDeclaration.Body is BlockSyntax body &&
            semanticModel.GetOperation(body, cts) is IBlockOperation bodyOperation)
        {
            var operations = bodyOperation.Operations;

            var returnStatements = operations
                .OfType<IReturnOperation>()
                .Select(ret => (ReturnStatementSyntax)ret.Syntax);
            
            newBody = body.ReplaceNodes(
                returnStatements,
                (node, _) => UpdateReturnStatement(node, name));

            var hasTrailingReturn = !operations.IsEmpty && operations[^1] is IReturnOperation;
            if (methodSymbol.ReturnsVoid && !hasTrailingReturn)
            {
                newBody = newBody.AddStatements(
                    SyntaxFactory.ReturnStatement(
                        SyntaxInator.OkCall(name)));
            }
        }

        var newMethodDeclaration = methodDeclaration
            .AddAttributeLists(
                returnsResultAttribute,
                resultDeclaration)
            .WithReturnType(newReturnType)
            .WithExpressionBody(newExpressionBody)
            .WithBody(newBody?.WithAdditionalAnnotations(Formatter.Annotation));

        var oldRoot = await document.GetSyntaxRootAsync(cts);
        var newRoot = oldRoot!.ReplaceNode(methodDeclaration, newMethodDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }

    private static ReturnStatementSyntax UpdateReturnStatement(
        ReturnStatementSyntax returnStatement,
        string resultTypeName)
    {
        var expression = SyntaxInator.WrapExpressionWithOk(
            resultTypeName,
            returnStatement.Expression);

        return returnStatement.WithExpression(expression);
    }
}
