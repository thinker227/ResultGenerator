using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultGenerator.Models;

internal readonly record struct ParameterType(
    string FullyQualifiedName,
    bool CanBeNull)
{
    public static ParameterType? Create(
        TypeSyntax syntax,
        SemanticModel semanticModel)
    {
        // Covered by diagnostic UnknownType.
        if (GetTypeSymbolInfo(syntax, semanticModel) is not ITypeSymbol symbol) return null;

        var fullyQualifiedName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // NOTE: For nullable value types, ITypeSymbol.IsReferenceType returns true.
        // This is bizarre but it helps streamline checking whether the type can be null.
        var canBeNull = symbol.IsReferenceType;

        return new(
            fullyQualifiedName,
            canBeNull);
    }

    public static ITypeSymbol? GetTypeSymbolInfo(
        TypeSyntax syntax,
        SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(syntax);
        var type = typeInfo.Type;

        // Filter out error types.
        return type is not IErrorTypeSymbol
            ? type
            : null;
    }
}
