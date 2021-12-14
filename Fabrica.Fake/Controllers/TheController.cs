using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.Api.Support.Controllers;
using Fabrica.Fake.Persistence;
using Fabrica.Mediator;
using Fabrica.Models.Patch.Builder;
using Fabrica.Persistence.Mediator;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Fabrica.Rql.Serialization;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Person = Fabrica.Fake.Persistence.Person;

namespace Fabrica.Fake.Controllers
{


    [Route("api")]
    public class TheController: BaseMediatorController
    {

        public TheController(ICorrelation correlation, IMessageMediator mediator, FakeReplicaDbContext context, IAmazonS3 client) : base(correlation, mediator)
        {

            Context = context;
            Client = client;

            Context.Database.EnsureCreated();

        }

        private FakeReplicaDbContext Context { get; }
        private IAmazonS3 Client { get; }


        [HttpGet("people")]
        public async Task<IActionResult> QueryPeople( [FromQuery] string rql )
        {

            using var logger = EnterMethod();


            var tree = RqlLanguageParser.ToCriteria(rql);
            var filter = new RqlFilterBuilder<Person>(tree);

            var predicate = filter.ToExpression();

            var query= Context.People.Where(predicate);
            var list = await query.ToListAsync();


            var result = Ok(list);

            
            return result;

        }


        [HttpGet("people/{uid}")]
        public async Task<IActionResult> RetrievePeople( string uid )
        {

            using var logger = EnterMethod();

            logger.Inspect(nameof(uid), uid);


            var person = await Context.People.SingleOrDefaultAsync(p => p.Uid == uid);

            logger.LogObject(nameof(person), person);


            return person is not null? Ok(person): NotFound();

        }


        [HttpPost("people")]
        public async Task<IActionResult> CreatePeople([FromBody] Dictionary<string, object> delta)
        {

            using var logger = EnterMethod();

            var request = new CreateEntityRequest<Person>
            {
                Delta = delta
            };

            var result = await Send(request);

            return result;

        }


        [HttpPut("people/{uid}")]
        public async Task<IActionResult> UpdatePeople(string uid, [FromBody] Dictionary<string,object> delta )
        {

            using var logger = EnterMethod();

            var request = new UpdateEntityRequest<Person>
            {
                Uid   = uid,
                Delta = delta
            };

            var result = await Send(request);

            return result;

        }


        [HttpGet("companies")]
        public async Task<IActionResult> QueryCompanies(  [FromQuery] string rql="" )
        {

            var tree = RqlLanguageParser.ToCriteria(rql);
            var filter = new RqlFilterBuilder<Company>(tree);

            var predicate = filter.ToExpression();

            var query = Context.Companies.Where(predicate);
            var list = await query.ToListAsync();


            var result = Ok(list);


            return result;


        }


        [HttpGet("companies/{uid}")]
        public async Task<IActionResult> RetrieveCompany(string uid)
        {

            using var logger = EnterMethod();

            logger.Inspect(nameof(uid), uid);


            var company = await Context.Companies.SingleOrDefaultAsync(p => p.Uid == uid);

            logger.LogObject( nameof(company), company );


            return company is not null ? Ok(company) : NotFound();

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






}
