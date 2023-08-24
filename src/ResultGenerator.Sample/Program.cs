using ResultGenerator;

Console.WriteLine("Hello, World!");

#pragma warning disable CS0658 // Disable invalid attribute target warnings.
public sealed class PersonService
{
    [ReturnsResult]
    [result: Created, DuplicateName]
    public void CreatePerson(string name, int age) =>
        throw new NotImplementedException();

    [ReturnsResult("GetPersonResult")]
    [result: Ok(Value<Person>), NotFound]
    public void GetPersonByName(string name) =>
        throw new NotImplementedException();

    public record Person(string Name, int Age);
}
