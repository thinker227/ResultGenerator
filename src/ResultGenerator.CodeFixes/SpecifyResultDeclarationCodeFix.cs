using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using ResultGenerator.Analysis;

namespace ResultGenerator.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class SpecifyResultDeclarationCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
        Diagnostics.SpecifyResultDeclaration.Id);

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext ctx)
    {
        var diagnostic = ctx.Diagnostics.First();
        var document = ctx.Document;

        var root = await document.GetSyntaxRootAsync();
        if (root is null) return;

        // The location of the diagnostic is always the identifier for the method.
        var node = root.FindNode(ctx.Span);
        if (node is not MethodDeclarationSyntax methodDeclaration) return;

        ctx.RegisterCodeFix(
            CodeAction.Create(
                "Add result declaration",
                cts => AddResultDeclaration(
                    cts,
                    document,
                    methodDeclaration)),
            diagnostic);
    }

    private async Task<Document> AddResultDeclaration(
        CancellationToken cts,
        Document document,
        MethodDeclarationSyntax methodDeclaration)
    {
        var leadingTrivia = methodDeclaration.GetLeadingTrivia();

        var attributeList = SyntaxInator.DefaultResultDeclaration(leadingTrivia)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newMethodDeclaration = methodDeclaration.AddAttributeLists(attributeList);

        var oldRoot = await document.GetSyntaxRootAsync(cts);
        var newRoot = oldRoot!.ReplaceNode(methodDeclaration, newMethodDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }
}
