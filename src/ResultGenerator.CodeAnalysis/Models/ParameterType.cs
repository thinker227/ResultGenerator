using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator.Models;

internal readonly record struct ParameterType(
    string FullyQualifiedName,
    bool IsNullable)
{
    public static ParameterType? Create(
        TypeSyntax syntax,
        SemanticModel semanticModel)
    {
        // Covered by diagnostic UnknownType.
        if (GetTypeSymbolInfo(syntax, semanticModel) is not INamedTypeSymbol symbol) return null;

        // If the type is nullable (i.e. a ? should be appended to it)
        // then the type syntax has to be a NullableTypeSyntax
        // and the type itself has to be a reference type,
        // otherwise it would refer to Nullable<T>.
        var isNullable =
            syntax is NullableTypeSyntax &&
            symbol.IsReferenceType;

        return new(
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            isNullable);
    }

    public static INamedTypeSymbol? GetTypeSymbolInfo(
        TypeSyntax syntax,
        SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(syntax);

        return typeInfo.Type as INamedTypeSymbol;
    }
}
