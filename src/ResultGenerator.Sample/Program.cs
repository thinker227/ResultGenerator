using ResultGenerator;

Console.WriteLine("Hello, World!");

public sealed class PersonService
{
    private readonly Dictionary<string, Person> people = new()
    {
        ["Max"] = new("Max", 22),
        ["Susie"] = new("Susie", 29),
    };

    [ReturnsResult]
    [result: Created, DuplicateName]
    // TODO: Update void to CreatePersonResult once that bug is fixed.
    public void CreatePerson(string name, int age)
    {
        var success = people.TryAdd(name, new(name, age));

        // return success
        //     ? CreatePersonResult.Created()
        //     : CreatePersonResult.DuplicateName();
    }

    [ReturnsResult("GetPersonResult")]
    [result: Ok(Value<Person>), NotFound]
    public GetPersonResult GetPersonByName(string name)
    {
        var person = name switch
        {
            "Max" => new Person("Max", 22),
            "Rick" => new Person("Rick", 36),
            "Susie" => new Person("Susie", 29),
            _ => null,
        };

        return person is not null
            ? GetPersonResult.Ok(person)
            : GetPersonResult.NotFound();
    }

    public record Person(string Name, int Age);
}
