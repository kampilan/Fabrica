using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Bogus;
using Bogus.DataSets;
using Fabrica.Exceptions;
using Fabrica.Rql;
using Fabrica.Rql.Serialization;
using Fabrica.Utilities.Text;
using Company = Fabrica.Fake.Controllers.Company;
using Person = Fabrica.Fake.Controllers.Person;

namespace Fabrica.Fake.Services;

public class FakeDataComponent: IStartable
{

    private ConcurrentBag<Person> People { get; set; }
    private ConcurrentBag<Company> Companies { get; set; }

    public void Start()
    {


        var ruleSetP = new Faker<Person>();

        ruleSetP
            .RuleFor(p => p.Uid, _ => Base62Converter.NewGuid())
            .RuleFor(p => p.Gender, f => f.Person.Random.Enum<Person.GenderKind>())
            .RuleFor(p => p.FirstName, (f, p) => f.Name.FirstName(p.Gender == Person.GenderKind.Female ? Name.Gender.Female : Name.Gender.Male))
            .RuleFor(p => p.MiddleName, (f, p) => f.Name.FirstName(p.Gender == Person.GenderKind.Female ? Name.Gender.Female : Name.Gender.Male))
            .RuleFor(p => p.LastName, f => f.Person.LastName)
            .RuleFor(p => p.BirthDate, f => f.Date.Past(90, DateTime.Now.AddYears(-18)))
            .RuleFor(p => p.Salary, f => f.Random.Decimal(20000, 500000))
            .RuleFor(p => p.Email, f => f.Person.Email)
            .RuleFor(p => p.PhoneNumber, f => f.Person.Phone);


        var people = ruleSetP.Generate(100000);

        People = new ConcurrentBag<Person>(people);


        var ruleSetC = new Faker<Company>();

        ruleSetC
            .RuleFor(p => p.Uid, _ => Base62Converter.NewGuid())
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.Address1, f => f.Address.StreetAddress())
            .RuleFor(c => c.Address2, f => f.Address.SecondaryAddress())
            .RuleFor(c => c.City, f => f.Address.City())
            .RuleFor(c => c.State, f => f.Address.StateAbbr())
            .RuleFor(c => c.Zip, f => f.Address.ZipCode())
            .RuleFor(c => c.MainPhone, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Fax, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Website, f => f.Internet.Url())
            .RuleFor(c => c.EmployeeCount, f => f.Random.Number(5, 50000));


        var companies = ruleSetC.Generate(100000);


        Companies = new ConcurrentBag<Company>(companies);



    }

    public IEnumerable<Person> QueryPeople(IRqlFilter<Person> filter)
    {

        var predicate = filter.ToLambda();

        var result = People.Where(predicate);

        return result;

    }

    public IEnumerable<Company> QueryCompanies(IRqlFilter<Company> filter)
    {

        var predicate = filter.ToLambda();

        var result = Companies.Where(predicate);

        return result;

    }


    public Person RetrievePerson( string uid )
    {


        var result = People.SingleOrDefault(p=>p.Uid == uid );
        if (result is null)
            throw new NotFoundException($"Could not find Person using Uid = ({uid})");


        return result;

    }

    public Company RetrieveCompany( string uid )
    {

        var result = Companies.SingleOrDefault(p => p.Uid == uid);
        if (result is null)
            throw new NotFoundException($"Could not find Company using Uid = ({uid})");


        return result;

    }


}