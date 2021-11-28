using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using Fabrica.Api.Support.Controllers;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Fabrica.Rql.Serialization;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Fake.Controllers
{


    [Route("api")]
    public class TheController: BaseController
    {

        public TheController(ICorrelation correlation) : base(correlation)
        {

        }


        [HttpGet("people")]
        public Task<IActionResult> GeneratePeople( [FromQuery] int count=1000, [FromQuery] string rql = "" )
        {

            using var logger = EnterMethod();
            
            var ruleSet = new Faker<Person>();

            ruleSet.RuleFor(p=>p.Uid, _=> ShortGuid.NewGuid().ToString() )
                .RuleFor(p => p.Gender, f => f.Person.Random.Enum<Person.GenderKind>())
                .RuleFor(p => p.FirstName, (f, p) => f.Name.FirstName(p.Gender == Person.GenderKind.Female ? Name.Gender.Female : Name.Gender.Male))
                .RuleFor(p => p.MiddleName, (f, p) => f.Name.FirstName(p.Gender == Person.GenderKind.Female ? Name.Gender.Female : Name.Gender.Male))
                .RuleFor(p => p.LastName, f => f.Person.LastName)
                .RuleFor(p => p.BirthDate, f => f.Date.Past(90, DateTime.Now.AddYears(-18)))
                .RuleFor(p => p.Salary, f => f.Random.Decimal(20000, 500000))
                .RuleFor(p => p.Email, f => f.Person.Email)
                .RuleFor(p => p.PhoneNumber, f => f.Person.Phone);


            var list = ruleSet.Generate(count);

            if( !string.IsNullOrWhiteSpace(rql) )
            {

                var tree = RqlLanguageParser.ToCriteria(rql);
                var filter = new RqlFilterBuilder<Person>(tree);
                var lambda = filter.ToLambda();

                var subset = list.Where(lambda);
                list = subset.ToList();

            }


            var result = Ok(list);

            return Task.FromResult((IActionResult)result);


        }


        [HttpGet("companies")]
        public Task<IActionResult> GenerateCompanies( [FromQuery] int count=100, [FromQuery] string rql="" )
        {

            using var logger = EnterMethod();

            var ruleSet = new Faker<Company>();

            ruleSet.RuleFor(p => p.Uid, _ => ShortGuid.NewGuid().ToString() )
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


            var list = ruleSet.Generate(count);

            if( !string.IsNullOrWhiteSpace(rql) )
            {

                var tree   = RqlLanguageParser.ToCriteria(rql);
                var filter = new RqlFilterBuilder<Company>(tree);
                var lambda = filter.ToLambda();

                var subset = list.Where(lambda);
                list = subset.ToList();

            }



            var result = Ok(list);

            return Task.FromResult((IActionResult)result);

        }





    }



    public class Person
    {

        public enum GenderKind { Female, Male }


        public string Uid { get; set; } = "";

        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public string LastName { get; set; } = "";

        public GenderKind Gender { get; set; } = GenderKind.Female;


        public DateTime BirthDate { get; set; } = DateTime.Now.AddYears(-25).Date;

        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";

        public decimal Salary { get; set; } = 0;

    }

    public class Company
    {


        public string Uid { get; set; } = "";

        public string Name { get; set; } = "";

        public string Address1 { get; set; } = "";
        public string Address2 { get; set; } = "";

        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string Zip { get; set; } = "";

        public string MainPhone { get; set; } = "";
        public string Fax { get; set; } = "";

        public string Website { get; set; } = "";

        public int EmployeeCount { get; set; } = 0;


    }




}
