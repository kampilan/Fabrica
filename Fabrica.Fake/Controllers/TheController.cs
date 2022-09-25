using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.Api.Support.Controllers;
using Fabrica.Fake.Persistence;
using Fabrica.Mediator;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Patch;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Fabrica.Rql.Serialization;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Person = Fabrica.Fake.Persistence.Person;

namespace Fabrica.Fake.Controllers;

[Route("/api")]
public class TheController: BaseMediatorController
{

    public TheController(ICorrelation correlation, IModelMetaService meta, IMessageMediator mediator, IPatchResolver resolver, FakeReplicaDbContext context, IAmazonS3 client) : base(correlation, meta, mediator)
    {

        Resolver = resolver;
        Context = context;
        Client = client;

        Context.Database.EnsureCreated();

    }

    private IPatchResolver Resolver { get; }
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


    [HttpPatch("people/{uid}")]
    public async Task<IActionResult> PatchPeople(string uid, [FromBody] List<ModelPatch> patches )
    {

        using var logger = EnterMethod();

        var set = new PatchSet();
        set.Add(patches);

        var requests = Resolver.Resolve(set);

        var response = await Mediator.Send(requests);

        response.EnsureSuccess();


        var request = RetrieveEntityRequest<Person>.ForUid(uid);

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


}