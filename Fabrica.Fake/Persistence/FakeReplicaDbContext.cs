using System;
using System.Collections.Generic;
using Bogus;
using Bogus.DataSets;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.Fake.Persistence;

public class FakeReplicaDbContext: ReplicaDbContext
{


    public static IEnumerable<Person> GetPeople()
    {

        long personId = 1000;

        var ruleSetP = new Faker<Person>();

        ruleSetP
            .RuleFor(p => p.IdSetter, _ => personId++)
            .RuleFor(p => p.Uid, _ => Base62Converter.NewGuid())
            .RuleFor(p => p.Gender, f => f.Person.Random.Enum<Person.GenderKind>())
            .RuleFor(p => p.FirstName, (f, p) => f.Name.FirstName(p.Gender == Person.GenderKind.Female ? Name.Gender.Female : Name.Gender.Male))
            .RuleFor(p => p.MiddleName, (f, p) => f.Name.FirstName(p.Gender == Person.GenderKind.Female ? Name.Gender.Female : Name.Gender.Male))
            .RuleFor(p => p.LastName, f => f.Person.LastName)
            .RuleFor(p => p.BirthDate, f => f.Date.Past(90, DateTime.Now.AddYears(-18)))
            .RuleFor(p => p.Salary, f => f.Random.Decimal(20000, 500000))
            .RuleFor(p => p.Email, f => f.Person.Email)
            .RuleFor(p => p.PhoneNumber, f => f.Person.Phone);


        var people = ruleSetP.Generate(10000);

        return people;

    }

    public static IEnumerable<Company> GetCompanies()
    {

        long companyId = 1000;

        var ruleSetC = new Faker<Company>();

        ruleSetC
            .RuleFor(p => p.IdSetter, _ => companyId++)
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


        var companies = ruleSetC.Generate(10000);

        return companies;

    }



    public FakeReplicaDbContext(ICorrelation correlation, DbContextOptions options, ILoggerFactory factory = null) : base(correlation, options, factory)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Person> People { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {

        mb.Entity<Company>().HasData(GetCompanies());

        mb.Entity<Person>().HasData(GetPeople());

        base.OnModelCreating(mb);

    }


}