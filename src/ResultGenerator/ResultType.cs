using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;

namespace ResultGenerator;

internal readonly record struct ResultTypeDeclarationAttribute(
    AttributeData Data,
    AttributeSyntax Syntax,
    AttributeCtorArgs ConstructArguments);

/// <summary>
/// Represents semantic and syntactic information about a result type.
/// </summary>
/// <param name="DeclaringMethod">The method which declares the result type.</param>
/// <param name="DeclaringAttribute">The <c>[ReturnsResult]</c> attribute on the declaring method.</param>
/// <param name="DeclarationSyntax">The syntax for the primary declaration.</param>
/// <param name="Name">The name of the result type.</param>
/// <param name="Values">The result values in the type.</param>
internal readonly record struct ResultType(
    IMethodSymbol DeclaringMethod,
    ResultTypeDeclarationAttribute DeclaringAttribute,
    AttributeListSyntax DeclarationSyntax,
    string Name,
    ImmutableArray<ResultValue> Values)
{
    /// <summary>
    /// Tries to create a new <see cref="ResultType"/>.
    /// </summary>
    /// <param name="method">The method to try create the type from.</param>
    /// <param name="typeProvider">A provider for well-known types.</param>
    /// <param name="semanticModel">The semantic model for the operation.</param>
    /// <param name="errorCallbacks">The error callbacks to use for reporting errors.</param>
    /// <param name="checkPartialDeclarations">Whether to check partial declarations of the method for potential errors.</param>
    /// <param name="parseInvalidDeclarations">Whether to parse invalid declarations for error checking.</param>
    /// <returns>The parsed result type,
    /// or <see langword="null"/> if it could not be parsed.</returns>
    public static ResultType? Create(
        IMethodSymbol method,
        WellKnownTypeProvider typeProvider,
        SemanticModel semanticModel,
        ResultTypeErrorCallbacks errorCallbacks,
        bool checkPartialDeclarations = true,
        bool parseInvalidDeclarations = true)
    {
        // Get attribute data.
        var attributeData = method
            .GetAttributeDataFor(typeProvider.ReturnsResultAttribute);
        if (attributeData is null) return null;

        return Create(
            method,
            attributeData,
            semanticModel,
            errorCallbacks,
            checkPartialDeclarations,
            parseInvalidDeclarations);
    }

    /// <summary>
    /// Tries to create a new <see cref="ResultType"/>.
    /// </summary>
    /// <param name="method">The method to try create the type from.</param>
    /// <param name="attributeData">The data for the declaring attribute.</param>
    /// <param name="semanticModel">The semantic model for the operation.</param>
    /// <param name="errorCallbacks">The error callbacks to use for reporting errors.</param>
    /// <param name="checkPartialDeclarations">Whether to check partial declarations of the method for potential errors.</param>
    /// <param name="parseInvalidDeclarations">Whether to parse invalid declarations for error checking.</param>
    /// <returns>The parsed result type,
    /// or <see langword="null"/> if it could not be parsed.</returns>
    public static ResultType? Create(
        IMethodSymbol method,
        AttributeData attributeData,
        SemanticModel semanticModel,
        ResultTypeErrorCallbacks errorCallbacks,
        bool checkPartialDeclarations = true,
        bool parseInvalidDeclarations = true)
    {
        // Only ordinary methods can be result type declarations.
        if (method.MethodKind is not MethodKind.Ordinary) return null;

        // Get method syntax.
        // Even if the method is partial, the declaring
        // syntax references are never more than one.
        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax) return null;

        // Get attribute syntax.
        // The application syntax reference *should* not be null.
        var attributeSyntax = (AttributeSyntax)
            attributeData.ApplicationSyntaxReference!.GetSyntax();
        
        if (checkPartialDeclarations) {
            var attributeMethodDeclaration = attributeSyntax
                .FirstAncestorOrSelf<MethodDeclarationSyntax>()!;

            // Check whether the method syntax the attribute was applied to is the
            // same as the method syntax which the method symbol represents.
            if (!methodSyntax.Equals(attributeMethodDeclaration)) return null;
        }

        var ctorArgs = AttributeCtorArgs.Create(attributeData);

        if (ctorArgs is null)
        {
            errorCallbacks.InvalidAttributeCtor?.Invoke(
                attributeData,
                // Since an empty ctor is a valid ctor, the argument cannot be null
                // if the returned ctor args are null.
                attributeSyntax.ArgumentList!);
            return null;
        }

        var name = Result.GetResultTypeName(ctorArgs, method);

        if (!Result.IsValidIdentifier(name))
        {
            errorCallbacks.InvalidTypeName?.Invoke(
                attributeData,
                attributeSyntax.ArgumentList!);
            return null!;
        }

        // Get result declarations.
        var declarations = methodSyntax
            .GetResultDeclarations()
            .ToImmutableArray();

        // To improve error handling, if there are multiple declarations
        // then the first one should be used.
        if (declarations is not [var declaration, ..])
        {
            errorCallbacks.NoDeclarations?.Invoke(attributeData, methodSyntax);
            return null;
        }
        // Only one result declaration is allowed per method.
        if (declarations.Length > 1)
        {
            errorCallbacks.MultipleDeclarations?.Invoke(declarations);

            if (parseInvalidDeclarations)
            {
                foreach (var errorDecl in declarations.Skip(1))
                {
                    // Run error checking for invalid declarations.
                    _ = ResultValue.ParseValues(
                        errorDecl,
                        semanticModel,
                        parseInvalidDeclarations,
                        errorCallbacks);
                }
            }
        }

        var values = ResultValue.ParseValues(
            declaration,
            semanticModel,
            parseInvalidDeclarations,
            errorCallbacks);

        return new(
            method,
            new(attributeData,
                attributeSyntax,
                ctorArgs),
            declaration,
            name,
            values);
    }
}
