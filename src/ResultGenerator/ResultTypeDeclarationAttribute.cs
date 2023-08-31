using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator;

/// <summary>
/// Represents a declaring attribute for a result type. 
/// </summary>
/// <param name="Data">The data for the attribute.</param>
/// <param name="Syntax">The syntax for the attribute.</param>
/// <param name="ConstructArguments">The constructor configuration of the attribute.</param>
internal readonly record struct ResultTypeDeclarationAttribute(
    AttributeData Data,
    AttributeSyntax Syntax,
    AttributeCtorArgs ConstructArguments);
