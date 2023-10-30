
using System.Collections.ObjectModel;
using System.Reflection;
using Fabrica.Watch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Api.Support.Endpoints;


public static class EndpointExtensions
{

    public static IServiceCollection AddEndpointModules(this IServiceCollection services, params Assembly[] sources)
    {

        var assemblies = new ReadOnlyCollection<Assembly>(sources);

        var modules = assemblies.SelectMany(x => x.GetTypes().Where(t => !t.IsAbstract && typeof(IEndpointModule).IsAssignableFrom(t) && t != typeof(IEndpointModule) && t.IsPublic));

        foreach( var newModule in modules )
        {
            services.AddSingleton(typeof(IEndpointModule), newModule);
        }


        return services;

    }


    public static IEndpointRouteBuilder MapEndpointModules(this IEndpointRouteBuilder builder)
    {

        var logger = WatchFactoryLocator.Factory.GetLogger("Fabrica.Api.Support.Endpoints");

        try
        {
            logger.EnterScope("MapEndpointModules");


            foreach (var moduleInterface in builder.ServiceProvider.GetServices<IEndpointModule>())
            {

                logger.Inspect( "EndpointModule", moduleInterface.GetType().Name );

                if( moduleInterface is BaseEndpointModule endpointModule )
                {

                    logger.Debug("Attempting to Configure group");

                    var group = builder.MapGroup(endpointModule.BasePath);

                    if (endpointModule.RequiresAuthorization)
                        group = group.RequireAuthorization(endpointModule.AuthorizationPolicyNames);
/*
                    if (!string.IsNullOrWhiteSpace(endpointModule.OpenApiDescription))
                        group = group.WithDescription(endpointModule.OpenApiDescription);

                    if (endpointModule.MetaData.Any())
                        group = group.WithMetadata(endpointModule.MetaData);

                    if (!string.IsNullOrWhiteSpace(endpointModule.OpenApiName))
                        group = group.WithName(endpointModule.OpenApiName);

                    if (!string.IsNullOrWhiteSpace(endpointModule.OpenApiSummary))
                        group = group.WithSummary(endpointModule.OpenApiSummary);

                    if (!string.IsNullOrWhiteSpace(endpointModule.OpenApiDisplayName))
                        group = group.WithDisplayName(endpointModule.OpenApiDisplayName);

                    if (!string.IsNullOrWhiteSpace(endpointModule.OpenApiGroupName))
                        group = group.WithGroupName(endpointModule.OpenApiGroupName);

                    if (endpointModule.Tags.Any())
                        group = group.WithTags(endpointModule.Tags);
*/

                    endpointModule.AddRoutes(group);

                }
                else
                {
                    logger.Debug("No Group");
                    moduleInterface.AddRoutes(builder);
                }
            }


        }
        finally
        {
            logger.LeaveScope("MapEndpointModules");
        }



        return builder;
    }



}

