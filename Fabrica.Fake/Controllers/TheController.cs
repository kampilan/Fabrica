using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.Api.Support.Controllers;
using Fabrica.Fake.Services;
using Fabrica.Models.Support;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Fake.Controllers
{


    [Route("api")]
    public class TheController: BaseController
    {

        public TheController(ICorrelation correlation, FakeDataComponent faker, IAmazonS3 client) : base(correlation)
        {

            Faker = faker;
            Client = client;

        }

        private FakeDataComponent Faker { get; }
        private IAmazonS3 Client { get; }


        [HttpGet("people")]
        public Task<IActionResult> QueryPeople( [FromQuery] string rql )
        {

            using var logger = EnterMethod();
            


            var tree = RqlLanguageParser.ToCriteria(rql);
            var filter = new RqlFilterBuilder<Person>(tree);

            var list = Faker.QueryPeople(filter);

            var result = Ok(list);

            
            return Task.FromResult((IActionResult)result);

        }


        [HttpGet("people/{uid}")]
        public Task<IActionResult> RetrievePeople( string uid )
        {

            using var logger = EnterMethod();


            var person = Faker.RetrievePerson(uid);

            var result = Ok(person);


            return Task.FromResult((IActionResult)result);

        }



        [HttpGet("companies")]
        public Task<IActionResult> GenerateCompanies( [FromQuery] int count=100, [FromQuery] string rql="" )
        {

            using var logger = EnterMethod();


            var tree = RqlLanguageParser.ToCriteria(rql);
            var filter = new RqlFilterBuilder<Company>(tree);

            var list = Faker.QueryCompanies(filter);

            var result = Ok(list);


            return Task.FromResult((IActionResult)result);


        }


        [HttpGet("companies/{uid}")]
        public Task<IActionResult> RetrieveCompany(string uid)
        {

            using var logger = EnterMethod();


            var company = Faker.RetrieveCompany(uid);

            var result = Ok(company);


            return Task.FromResult((IActionResult)result);

        }


        [HttpPost("s3-events/{topic}")]
        public async Task<StatusCodeResult> AcceptWorkRequest( [FromRoute] string topic, [FromBody] S3Event s3 )
        {

            using var logger = EnterMethod();

            logger.Inspect(nameof(topic), topic);
            logger.LogObject(nameof(s3), s3);



            // *****************************************************************
            logger.Debug("Attempting to build delete object request");
            var request = new DeleteObjectRequest
            {
                BucketName = s3.Bucket,
                Key         = s3.Key
            };



            // *****************************************************************
            logger.Debug("Attempting to send request to S3");
            var response = await Client.DeleteObjectAsync(request);

            logger.LogObject(nameof(response), response);



            // *****************************************************************
            return Ok();

        }

        [HttpPost("s3-events-bad/{topic}")]
        public Task<StatusCodeResult> AcceptBadWorkRequest([FromRoute] string topic, [FromBody] S3Event s3)
        {

            using var logger = EnterMethod();

            logger.Inspect(nameof(topic), topic);
            logger.LogObject(nameof(s3), s3);

            return Task.FromResult(new StatusCodeResult(422));

        }

        [HttpPost("s3-events-transient/{topic}")]
        public Task<StatusCodeResult> AcceptTransientWorkRequest([FromRoute] string topic, [FromBody] S3Event s3)
        {

            using var logger = EnterMethod();

            logger.Inspect(nameof(topic), topic);
            logger.LogObject(nameof(s3), s3);

            return Task.FromResult(new StatusCodeResult(500));

        }



    }

    public class S3Event
    {

        public string Region { get; set; }
        public string Bucket { get; set; }
        public string Key { get; set; }

        public long Size { get; set; }
        

    }


    public class Person: BaseMutableModel<Person>, IRootModel, IExplorableModel
    {

        public enum GenderKind { Female, Male }

        private long _id;
        public override long Id
        {
            get=>_id;
            protected set { }
        }

        public override string Uid { get; set; } = "";

        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public string LastName { get; set; } = "";

        public GenderKind Gender { get; set; } = GenderKind.Female;


        public DateTime BirthDate { get; set; } = DateTime.Now.AddYears(-25).Date;

        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";

        public decimal Salary { get; set; } = 0;

    }

    public class Company: BaseMutableModel<Company>, IRootModel, IExplorableModel
    {

        private long _id;
        public override long Id
        {
            get => _id;
            protected set { }
        }

        public override string Uid { get; set; } = "";

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
