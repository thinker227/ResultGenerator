namespace ResultGenerator.Helpers;

public static class EnumerableHelpers
{
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> xs) where T : struct
    {
        foreach (var x in xs)
        {
            if (x is T value)
                yield return value;
        }
    }
}
