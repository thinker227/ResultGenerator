using System.Diagnostics;
using ResultGenerator;

var personService = new PersonService();

personService.CreatePerson("Max", 22);

var maxResult = personService.GetPersonByName("Max");
Debug.Assert(maxResult.IsOk);

maxResult.TryAsOk(out var max);
Debug.Assert(max!.Name == "Max");
Debug.Assert(max!.Age == 22);

var susie = personService.GetPersonByName("Susie");
Debug.Assert(susie.IsNotFound);

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
