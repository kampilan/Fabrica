/*

MIT License

Copyright (c) 2017 Jonathan Channon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 
*/


namespace Fabrica.Api.Support.Endpoints;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Negotiation;
using OpenApi;
using Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class EndpointExtensions
{
    /// <summary>
    /// Adds Carter to the specified <see cref="IApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IEndpointRouteBuilder MapCarter(this IEndpointRouteBuilder builder)
    {
        var loggerFactory = builder.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(EndpointConfigurator));

        var carterConfigurator = builder.ServiceProvider.GetRequiredService<EndpointConfigurator>();
        carterConfigurator.LogDiscoveredCarterTypes(logger);

        foreach (var carterModuleInterface in builder.ServiceProvider.GetServices<IEndpointModule>())
        {
            if (carterModuleInterface is AbstractEndpointModule carterModule)
            {
                var group = builder.MapGroup(carterModule.BasePath);

                if (carterModule.hosts.Any())
                {
                    group = group.RequireHost(carterModule.hosts);
                }

                if (carterModule.requiresAuthorization)
                {
                    group = group.RequireAuthorization(carterModule.authorizationPolicyNames);
                }

                if (!string.IsNullOrWhiteSpace(carterModule.corsPolicyName))
                {
                    group = group.RequireCors(carterModule.corsPolicyName);
                }

                if (carterModule.includeInOpenApi)
                {
                    group.IncludeInOpenApi();
                }

                if (!string.IsNullOrWhiteSpace(carterModule.openApiDescription))
                {
                    group = group.WithDescription(carterModule.openApiDescription);
                }

                if (carterModule.metaData.Any())
                {
                    group = group.WithMetadata(carterModule.metaData);
                }

                if (!string.IsNullOrWhiteSpace(carterModule.openApiName))
                {
                    group = group.WithName(carterModule.openApiName);
                }

                if (!string.IsNullOrWhiteSpace(carterModule.openApisummary))
                {
                    group = group.WithSummary(carterModule.openApisummary);
                }

                if (!string.IsNullOrWhiteSpace(carterModule.openApiDisplayName))
                {
                    group = group.WithDisplayName(carterModule.openApiDisplayName);
                }

                if (!string.IsNullOrWhiteSpace(carterModule.openApiGroupName))
                {
                    group = group.WithGroupName(carterModule.openApiGroupName);
                }

                if (carterModule.tags.Any())
                {
                    group = group.WithTags(carterModule.tags);
                }

                if (!string.IsNullOrWhiteSpace(carterModule.cacheOutputPolicyName))
                {
                    group = group.CacheOutput(carterModule.cacheOutputPolicyName);
                }

                if (carterModule.disableRateLimiting)
                {
                    group = group.DisableRateLimiting();
                }

                if (!string.IsNullOrWhiteSpace(carterModule.rateLimitingPolicyName))
                {
                    group = group.RequireRateLimiting(carterModule.rateLimitingPolicyName);
                }

                if (carterModule.Before != null)
                {
                    group.AddEndpointFilter(async (context, next) =>
                    {
                        var result = carterModule.Before.Invoke(context);
                        if (result != null)
                        {
                            return result;
                        }

                        return await next(context);
                    });
                }

                if (carterModule.After != null)
                {
                    group.AddEndpointFilter(async (context, next) =>
                    {
                        var result = await next(context);
                        carterModule.After.Invoke(context);
                        return result;
                    });
                }

                carterModule.AddRoutes(group);
            }
            else
            {
                carterModuleInterface.AddRoutes(builder);
            }
        }

        return builder;
    }

    /// <summary>
    /// Adds Carter to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add Carter to.</param>
    /// <param name="assemblyCatalog">Optional <see cref="DependencyContextAssemblyCatalog"/> containing assemblies to add to the services collection. If not provided, the default catalog of assemblies is added, which includes Assembly.GetEntryAssembly.</param>
    /// <param name="configurator">Optional <see cref="EndpointConfigurator"/> to enable registration of specific types within Carter</param>
    public static IServiceCollection AddCarter(this IServiceCollection services, DependencyContextAssemblyCatalog assemblyCatalog = null, Action<EndpointConfigurator> configurator = null)
    {
        assemblyCatalog ??= new DependencyContextAssemblyCatalog();

        var config = new EndpointConfigurator();
        configurator?.Invoke(config);

        services.WireupCarter(assemblyCatalog, config);

        return services;
    }

    private static void WireupCarter(this IServiceCollection services, DependencyContextAssemblyCatalog assemblyCatalog, EndpointConfigurator endpointConfigurator )
    {
        var assemblies = assemblyCatalog.GetAssemblies();

        var newModules = GetNewModules(endpointConfigurator, assemblies);

        //var modules = GetModules(endpointConfigurator, assemblies);

        var responseNegotiators = GetResponseNegotiators(endpointConfigurator, assemblies);

        services.AddSingleton(endpointConfigurator);


        foreach (var newModule in newModules)
        {
            services.AddSingleton(typeof(IEndpointModule), newModule);
        }

        // foreach (var newModule in modules)
        // {
        //     services.AddSingleton(typeof(CarterModule), newModule);
        // }

        foreach (var negotiator in responseNegotiators)
        {
            services.AddSingleton(typeof(IResponseNegotiator), negotiator);
        }

        services.AddSingleton<IResponseNegotiator, DefaultJsonResponseNegotiator>();
    }

    private static IEnumerable<Type> GetResponseNegotiators(EndpointConfigurator endpointConfigurator, IReadOnlyCollection<Assembly> assemblies)
    {
        IEnumerable<Type> responseNegotiators;
        if (endpointConfigurator.ExcludeResponseNegotiators || endpointConfigurator.ResponseNegotiatorTypes.Any())
        {
            responseNegotiators = endpointConfigurator.ResponseNegotiatorTypes;
        }
        else
        {
            responseNegotiators = assemblies.SelectMany(x => x.GetTypes()
                .Where(t =>
                    !t.IsAbstract &&
                    typeof(IResponseNegotiator).IsAssignableFrom(t) &&
                    t != typeof(IResponseNegotiator) &&
                    t != typeof(DefaultJsonResponseNegotiator)
                ));

            endpointConfigurator.ResponseNegotiatorTypes.AddRange(responseNegotiators);
        }

        return responseNegotiators;
    }

    private static IEnumerable<Type> GetNewModules(EndpointConfigurator endpointConfigurator, IReadOnlyCollection<Assembly> assemblies)
    {
        IEnumerable<Type> modules;
        if (endpointConfigurator.ExcludeModules || endpointConfigurator.ModuleTypes.Any())
        {
            modules = endpointConfigurator.ModuleTypes;
        }
        else
        {
            modules = assemblies.SelectMany(x => x.GetTypes()
                .Where(t =>
                    !t.IsAbstract &&
                    typeof(IEndpointModule).IsAssignableFrom(t) &&
                    t != typeof(IEndpointModule) &&
                    t.IsPublic
                ));

            endpointConfigurator.ModuleTypes.AddRange(modules);
        }

        return modules;
    }

    private static IEnumerable<Type> GetModules(EndpointConfigurator endpointConfigurator, IReadOnlyCollection<Assembly> assemblies)
    {
        // IEnumerable<Type> modules;
        // if (endpointConfigurator.ExcludeModules || endpointConfigurator.ModuleTypes.Any())
        // {
        //     modules = endpointConfigurator.ModuleTypes;
        // }
        // else
        //{
        var modules = assemblies.SelectMany(x => x.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                typeof(AbstractEndpointModule).IsAssignableFrom(t) &&
                t != typeof(AbstractEndpointModule) &&
                t.IsPublic
            ));

        //endpointConfigurator.ModuleTypes.AddRange(modules);
        //}

        return modules;
    }

    private class CompositeConventionBuilder : IEndpointConventionBuilder
    {
        private readonly List<IEndpointConventionBuilder> _builders;

        public CompositeConventionBuilder(List<IEndpointConventionBuilder> builders)
        {
            _builders = builders;
        }

        public void Add(Action<EndpointBuilder> convention)
        {
            foreach (var builder in _builders)
            {
                builder.Add(convention);
            }
        }
    }
}
