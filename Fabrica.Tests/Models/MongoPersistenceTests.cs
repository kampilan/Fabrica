using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Bogus;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Models.Support;
using Fabrica.Persistence;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Mongo;
using Fabrica.Persistence.Mongo.Mediator.Handlers;
using Fabrica.Persistence.Patch;
using Fabrica.Rules;
using Fabrica.Test.Models;
using Fabrica.Test.Models.Patch;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Text;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;
using IContainer = Autofac.IContainer;
using Module = Autofac.Module;

namespace Fabrica.Tests.Models;

public class MongoPersistenceTests
{

    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.BurlyWood);

        maker.Build();


        var builder = new ContainerBuilder();

        builder.RegisterModule<TheMongoPersistenceModule>();

        TheContainer = await builder.BuildAndStart();


    }

    [OneTimeTearDown]
    public void Teardown()
    {

        TheContainer.Dispose();
        WatchFactoryLocator.Factory.Stop();

    }

    private IContainer TheContainer { get; set; }

    private MongoCompany _buildCompany(int employees, bool asNew = false)
    {

        var compRules = new Faker<MongoCompany>();

        compRules
            .RuleFor(p => p.Uid, _ => Base62Converter.NewGuid())
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.Address1, f => f.Address.StreetAddress())
            .RuleFor(c => c.Address2, f => f.Address.SecondaryAddress())
            .RuleFor(c => c.City, f => f.Address.City())
            .RuleFor(c => c.State, f => f.Address.State())
            .RuleFor(c => c.Zip, f => f.Address.ZipCode());


        var company = new MongoCompany(asNew);

        compRules.Populate(company);

        var personRules = new Faker<Test.Models.Patch.Person>();

        personRules
            .RuleFor(p => p.Uid, _ => Base62Converter.NewGuid())
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName());


        if (!asNew)
            company.Post();

        return company;

    }


    [Test]
    public async Task Test_0505_100_MongoCrud()
    {


        await using var scope = TheContainer.BeginLifetimeScope();

        var corr = scope.Resolve<ICorrelation>();
        var rl = scope.Resolve<IRuleSet>();
        var mc = scope.Resolve<IMongoDbContext>();
        var am = scope.Resolve<IMapper>();

        var company = _buildCompany(0, true);


        var reqC = new CreateEntityRequest<MongoCompany>();
        reqC.FromObject(company);

        var hc = new CreateCompanyEntityHandler(corr, mc, am);

        var resC = await hc.Handle(reqC,new CancellationToken());


        Assert.IsNotNull(resC);
        Assert.IsTrue(resC.Ok);
        Assert.IsNotNull(resC.Value);


        var reqQ = new QueryEntityRequest<MongoCompany>();
        reqQ.Where(c => c.Name).Equals(company.Name).And(c => c.State).Equals(company.State);

        var hq = new QueryCompanyEntityHandler( corr, rl, mc );

        var resQ = await hq.Handle(reqQ, new CancellationToken());


        Assert.IsNotNull(resQ);
        Assert.IsTrue(resC.Ok);
        Assert.IsNotNull(resQ.Value);
        Assert.IsNotEmpty(resQ.Value);


        var compQ = resQ.Value.SingleOrDefault();
        Assert.IsNotNull(compQ);

        var reqR = new RetrieveEntityRequest<MongoCompany>
        {
            Uid = compQ.Uid
        };

        var hr = new RetrieveCompanyEntityHandler(corr, mc);
        var resR = await hr.Handle(reqR,new CancellationToken());

        Assert.IsNotNull(resR);
        Assert.IsTrue(resR.Ok);
        Assert.IsNotNull(resR.Value);

        var compU = resR.Value;

        compU.Address1 = "618 Valley View Dr";

        var reqU = new UpdateEntityRequest<MongoCompany>
        {
            Uid = compU.Uid
        };
        reqU.FromObject(compU);

        var hu = new UpdateCompanyEntityHandler(corr, mc, am);

        var resU = await hu.Handle(reqU, new CancellationToken());

        Assert.IsNotNull(resU);
        Assert.IsTrue(resU.Ok);
        Assert.IsNotNull(resU.Value);



        var reqD = new DeleteEntityRequest<MongoCompany>
        {
            Uid = compQ.Uid
        };

        var hd = new DeleteCompanyEntityHandler(corr, mc);
        var resD = await hd.Handle(reqD, new CancellationToken());

        Assert.IsNotNull(resD);
        Assert.IsTrue(resD.Ok);

        var resPd = await hr.Handle(reqR, new CancellationToken());

        Assert.IsNotNull(resPd);
        Assert.IsFalse(resPd.Ok);



    }

    [Test]
    public async Task Test_0505_200_MongoPatch()
    {

        await using var scope = TheContainer.BeginLifetimeScope();


        var company = _buildCompany(0, true);

        var mediator = scope.Resolve<IMessageMediator>();
        var resolver = scope.Resolve<IPatchResolver>();
        
        var reqC = resolver.Resolve(company);

        Assert.IsNotNull(reqC);
        
        var resC = await mediator.Send(reqC);

        Assert.IsNotNull(resC);
        Assert.IsFalse( resC.HasErrors );


        var reqR = new RetrieveEntityRequest<MongoCompany>
        {
            Uid = company.Uid
        };

        var resR = await mediator.Send(reqR);

        Assert.IsNotNull(resR);
        Assert.IsTrue(resR.Ok);
        Assert.IsNotNull(resR.Value);


        var compU = resR.Value;
        compU.Post();

        compU.Address1 = "618 Valley View Dr";

        var reqU = resolver.Resolve(compU);

        Assert.IsNotNull(reqU);

        var resU = await mediator.Send(reqU);

        Assert.IsNotNull(resU);
        Assert.IsFalse(resU.HasErrors);



        compU.Post();
        compU.Removed();
        var reqD = resolver.Resolve(compU);

        Assert.IsNotNull(reqD);

        var resD = await mediator.Send(reqD);


        Assert.IsNotNull(resD);
        Assert.IsFalse(resD.HasErrors);



        var reqPd = new QueryEntityRequest<MongoCompany>();
        reqPd.All();

        var resPd = await mediator.Send(reqPd);

        Assert.IsNotNull(resPd);
        Assert.IsNotNull(resPd.Value);
        Assert.IsEmpty(resPd.Value);



    }

}

public class QueryCompanyEntityHandler : BaseQueryHandler<QueryEntityRequest<MongoCompany>, MongoCompany>
{
    public QueryCompanyEntityHandler(ICorrelation correlation, IRuleSet rules, IMongoDbContext context) : base(correlation, rules, context)
    {
    }
}



public class RetrieveCompanyEntityHandler : BaseRetrieveHandler<RetrieveEntityRequest<MongoCompany>, MongoCompany>
{
    public RetrieveCompanyEntityHandler(ICorrelation correlation, IMongoDbContext context) : base(correlation, context)
    {
    }
}


public class CreateCompanyEntityHandler: BaseCreateHandler<CreateEntityRequest<MongoCompany>,MongoCompany>
{
    public CreateCompanyEntityHandler( ICorrelation correlation, IMongoDbContext context, IMapper mapper ) : base(correlation, context, mapper)
    {
    }
}


public class UpdateCompanyEntityHandler : BaseUpdateHandler<UpdateEntityRequest<MongoCompany>, MongoCompany>
{
    public UpdateCompanyEntityHandler(ICorrelation correlation, IMongoDbContext context, IMapper mapper) : base(correlation, context, mapper)
    {
    }
}


public class DeleteCompanyEntityHandler : BaseDeleteHandler<DeleteEntityRequest<MongoCompany>, MongoCompany>
{
    public DeleteCompanyEntityHandler(ICorrelation correlation, IMongoDbContext context) : base( correlation, context )
    {
    }
}





public class TheMongoPersistenceModule : Module
{

    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();

        builder.UseRules()
            .AddRules(GetType().Assembly);

        builder.RegisterAutoMapper(GetType().Assembly);

        builder.UseMongoDb("mongodb://mongodb.fabricatio.io:27017", "testing");

        builder.UseModelMeta().AddModelMetaSource(GetType().Assembly, typeof(IAssemblyFinder).Assembly);

        builder.UseMediator(GetType().Assembly);
        builder.UsePatchResolver();

    }


}

