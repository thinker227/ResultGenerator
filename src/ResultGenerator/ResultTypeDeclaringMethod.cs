using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultGenerator.Helpers;

namespace ResultGenerator;

/// <summary>
/// Represents information about a method which declares a result type.
/// </summary>
/// <param name="DeclaringMethod">The declaring method.</param>
/// <param name="DeclaringAttribute">The <c>[ReturnsResult]</c> attribute on the declaring method.</param>
/// <param name="Name">The name of the result type.</param>
/// <param name="DeclarationSyntaxes">A list of syntax for each declaration.</param>
internal readonly record struct ResultTypeDeclaringMethod(
    IMethodSymbol DeclaringMethod,
    ResultTypeDeclarationAttribute DeclaringAttribute,
    string Name,
    ImmutableArray<AttributeListSyntax> DeclarationSyntaxes)
{
    /// <summary>
    /// Tries to create a new <see cref="ResultTypeDeclaringMethod"/>.
    /// </summary>
    /// <param name="method">The method to try create the declaration from.</param>
    /// <param name="typeProvider">A provider for well-known type.</param>
    /// <param name="diagnostics">A list of diagnostics.</param>
    /// <param name="checkPartialDeclarations">Whether to check partial declarations
    /// of the method for potential errors.</param>
    /// <returns>The parsed declaration info,
    /// or <see langword="null"/> if it could not be parsed.</returns>
    public static ResultTypeDeclaringMethod? Create(
        IMethodSymbol method,
        WellKnownTypeProvider typeProvider,
        ImmutableArray<Diagnostic>.Builder? diagnostics,
        bool checkPartialDeclarations = true)
    {
        // Get attribute data.
        var attributeData = method
            .GetAttributeDataFor(typeProvider.ReturnsResultAttribute);
        if (attributeData is null) return null;
        
        return Create(
            method,
            attributeData,
            diagnostics,
            checkPartialDeclarations);
    }
    
    /// <summary>
    /// Tries to create a new <see cref="ResultTypeDeclaringMethod"/>.
    /// </summary>
    /// <param name="method">The method to try create the declaration from.</param>
    /// <param name="attributeData">The data for the declaring attribute.</param>
    /// <param name="diagnostics">A list of diagnostics.</param>
    /// <param name="checkPartialDeclarations">Whether to check partial declarations
    /// of the method for potential errors.</param>
    /// <returns>The parsed declaration info,
    /// or <see langword="null"/> if it could not be parsed.</returns>
    public static ResultTypeDeclaringMethod? Create(
        IMethodSymbol method,
        AttributeData attributeData,
        ImmutableArray<Diagnostic>.Builder? diagnostics,
        bool checkPartialDeclarations = true)
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
        
        if (checkPartialDeclarations)
        {
            var attributeMethodDeclaration = attributeSyntax
                .FirstAncestorOrSelf<MethodDeclarationSyntax>()!;

            // Check whether the method syntax the attribute was applied to is the
            // same as the method syntax which the method symbol represents.
            if (!methodSyntax.Equals(attributeMethodDeclaration)) return null;
        }

        // Parse attribute constructor arguments.
        var ctorArgs = AttributeCtorArgs.Create(attributeData);

        if (ctorArgs is null)
        {
            // Since an empty ctor is a valid ctor, the arguments cannot be null
            // if the returned ctor args are null.
            var location = attributeSyntax.ArgumentList!.GetLocation();

            diagnostics?.Add(Diagnostic.Create(
                Diagnostics.InvalidAttributeCtor,
                location));
            
            return null;
        }

        // Get name.
        var name = Result.GetResultTypeName(ctorArgs, method);

        if (!Result.IsValidIdentifier(name))
        {
            var nameSyntax = attributeSyntax.ArgumentList!.Arguments[0].Expression;
            var location = nameSyntax.GetLocation();

            diagnostics?.Add(Diagnostic.Create(
                Diagnostics.InvalidResultTypeName,
                location,
                nameSyntax));
            
            return null;
        }

        // Get result declarations.
        var declarations = methodSyntax
            .GetResultDeclarations()
            .ToImmutableArray();

        switch (declarations.Length)
        {
        case 0:
            // If there is no declaration, there is nothing more to do.
            diagnostics?.Add(Diagnostic.Create(
                Diagnostics.SpecifyResultDeclaration,
                methodSyntax.Identifier.GetLocation()));
            return null;
        case > 1:
            // Only one result declaration is allowed per method.
            foreach (var errorDecl in declarations.Skip(1))
            {
                diagnostics?.Add(Diagnostic.Create(
                    Diagnostics.TooManyResultDeclarations,
                    errorDecl.GetLocation()));
            }

            break;
        }

        return new(
            method,
            new(attributeData,
                attributeSyntax,
                ctorArgs),
            name,
            declarations);
    }

    public ResultType ToResultType(
        SemanticModel semanticModel,
        ImmutableArray<Diagnostic>.Builder? diagnostics,
        bool parseInvalidDeclarations = true)
    {
        // To improve error handling, if there are multiple declarations
        // then the first one should be used.
        var declaration = DeclarationSyntaxes[0];
        
        // Parse values.
        var values = ResultValue.ParseValues(
            declaration,
            semanticModel,
            diagnostics,
            parseInvalidDeclarations);
        
        // Only one result declaration is allowed per method.
        foreach (var errorDecl in DeclarationSyntaxes.Skip(1))
        {
            diagnostics?.Add(Diagnostic.Create(
                Diagnostics.TooManyResultDeclarations,
                errorDecl.GetLocation()));
            
            // Run error checking for invalid declarations.
            _ = ResultValue.ParseValues(
                errorDecl,
                semanticModel,
                diagnostics,
                parseInvalidDeclarations);
        }

        return new(
            DeclaringMethod,
            DeclaringAttribute,
            declaration,
            Name,
            values);
    }
}
