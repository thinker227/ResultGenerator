using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator;

/// <summary>
/// Container for callbacks for errors occuring when trying to parse a result type.
/// </summary>
internal readonly record struct ResultTypeErrorCallbacks(
    Action<AttributeData, AttributeArgumentListSyntax>? InvalidAttributeCtor = null,
    Action<AttributeData, AttributeArgumentListSyntax>? InvalidTypeName = null,
    Action<AttributeData, MethodDeclarationSyntax>? NoDeclarations = null,
    Action<ImmutableArray<AttributeListSyntax>>? MultipleDeclarations = null,
    Action<AttributeSyntax, NameSyntax>? InvalidValueNameSyntax = null,
    Action<AttributeArgumentSyntax, ExpressionSyntax>? BadValueParameterSyntax = null,
    Action<AttributeArgumentSyntax, ExpressionSyntax, SeparatedSyntaxList<TypeSyntax>>? TooManyValueParameterTypes = null,
    Action<TypeSyntax>? UnknownType = null);
