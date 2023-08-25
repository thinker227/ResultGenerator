using ResultGenerator;

var personService = new PersonService();

personService.CreatePerson("Max", 22);
personService.CreatePerson("Susie", 29);

public sealed class PersonService
{
    private readonly Dictionary<string, Person> people = new();

    [ReturnsResult]
    [result: Created, DuplicateName]
    public CreatePersonResult CreatePerson(string name, int age)
    {
        var success = people.TryAdd(name, new(name, age));

        return success
            ? CreatePersonResult.Created()
            : CreatePersonResult.DuplicateName();
    }

    [ReturnsResult("GetPersonResult")]
    [result: Ok(Value<Person>), NotFound]
    public GetPersonResult GetPersonByName(string name)
    {
        if (people.TryGetValue(name, out var person))
            return GetPersonResult.Ok(person);

        return GetPersonResult.NotFound();
    }

    public record Person(string Name, int Age);
}
