using Microsoft.CodeAnalysis.CSharp;

namespace ResultGenerator.Helpers;

public static class SyntaxUtility
{
    public static bool IsValidIdentifier(string identifier) =>
        identifier.Length >= 1 &&
        SyntaxFacts.IsIdentifierStartCharacter(identifier[0]) &&
        identifier.Skip(1).All(SyntaxFacts.IsIdentifierPartCharacter);
}
