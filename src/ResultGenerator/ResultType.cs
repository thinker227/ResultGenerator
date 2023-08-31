using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator;

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
    ImmutableArray<ResultValue> Values);
