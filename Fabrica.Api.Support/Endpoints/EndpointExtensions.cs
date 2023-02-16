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

using Fabrica.Api.Support.Endpoints.Module;

using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Api.Support.Endpoints;


public static class EndpointExtensions
{
    
    /// <summary>
    /// Adds Carter to the specified <see cref="IApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IEndpointRouteBuilder MapEndpointModules(this IEndpointRouteBuilder builder)
    {

        foreach (var moduleInterface in builder.ServiceProvider.GetServices<IEndpointModule>())
        {

            if (moduleInterface is AbstractEndpointModule endpointModule)
            {

                var group = builder.MapGroup(endpointModule.BasePath);

                if (endpointModule.hosts.Any())
                {
                    group = group.RequireHost(endpointModule.hosts);
                }

                if (endpointModule.requiresAuthorization)
                {
                    group = group.RequireAuthorization(endpointModule.authorizationPolicyNames);
                }

                if (!string.IsNullOrWhiteSpace(endpointModule.corsPolicyName))
                {
                    group = group.RequireCors(endpointModule.corsPolicyName);
                }

                if (!string.IsNullOrWhiteSpace(endpointModule.openApiDescription))
                {
                    group = group.WithDescription(endpointModule.openApiDescription);
                }

                if (endpointModule.metaData.Any())
                {
                    group = group.WithMetadata(endpointModule.metaData);
                }

                if (!string.IsNullOrWhiteSpace(endpointModule.openApiName))
                {
                    group = group.WithName(endpointModule.openApiName);
                }

                if (!string.IsNullOrWhiteSpace(endpointModule.openApisummary))
                {
                    group = group.WithSummary(endpointModule.openApisummary);
                }

                if (!string.IsNullOrWhiteSpace(endpointModule.openApiDisplayName))
                {
                    group = group.WithDisplayName(endpointModule.openApiDisplayName);
                }

                if (!string.IsNullOrWhiteSpace(endpointModule.openApiGroupName))
                {
                    group = group.WithGroupName(endpointModule.openApiGroupName);
                }

                if (endpointModule.tags.Any())
                {
                    group = group.WithTags(endpointModule.tags);
                }

                if (!string.IsNullOrWhiteSpace(endpointModule.cacheOutputPolicyName))
                {
                    group = group.CacheOutput(endpointModule.cacheOutputPolicyName);
                }

                if (endpointModule.disableRateLimiting)
                {
                    group = group.DisableRateLimiting();
                }

                if (!string.IsNullOrWhiteSpace(endpointModule.rateLimitingPolicyName))
                {
                    group = group.RequireRateLimiting(endpointModule.rateLimitingPolicyName);
                }

                if (endpointModule.Before != null)
                {
                    group.AddEndpointFilter(async (context, next) =>
                    {
                        var result = endpointModule.Before.Invoke(context);
                        if (result != null)
                        {
                            return result;
                        }

                        return await next(context);
                    });
                }

                if (endpointModule.After != null)
                {
                    group.AddEndpointFilter(async (context, next) =>
                    {
                        var result = await next(context);
                        endpointModule.After.Invoke(context);
                        return result;
                    });
                }

                endpointModule.AddRoutes(group);
            }
            else
            {
                moduleInterface.AddRoutes(builder);
            }
        }

        return builder;
    }

    public static IServiceCollection AddEndpointModules(this IServiceCollection services, params Assembly[] sources )
    {

        var assemblies = new ReadOnlyCollection<Assembly>( sources );

        var modules = assemblies.SelectMany(x => x.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                typeof(IEndpointModule).IsAssignableFrom(t) &&
                t != typeof(IEndpointModule) &&
                t.IsPublic
            ));


        foreach (var newModule in modules)
        {
            services.AddSingleton(typeof(IEndpointModule), newModule);
        }


        return services;

    }


}
