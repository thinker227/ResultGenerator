using ResultGenerator;

Console.WriteLine("Hello, World!");

S.Hello();

#pragma warning disable CS0658 // Disable invalid attribute target warnings.
internal static class C
{
    [ReturnsResult]
    [result: TooSmall, TooLarge, Ok]
    public static void GetNumber(int x) =>
        throw new NotImplementedException();

    [ReturnsResult]
    [result: NotFound, Ok(Value<Person>)]
    public static void GetPerson(string name) =>
        throw new NotImplementedException();

    public record Person(string Name, int Age);
}
