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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;

public abstract class AbstractEndpointModule : IEndpointModule
{
    internal string[] hosts = Array.Empty<string>();

    internal string corsPolicyName;

    internal string openApiDescription;

    internal object[] metaData = Array.Empty<object>();

    internal string openApiName;

    internal string openApisummary;

    internal string openApiDisplayName;

    internal string openApiGroupName;

    internal string[] tags = Array.Empty<string>();

    internal bool includeInOpenApi;

    internal bool requiresAuthorization;

    internal string[] authorizationPolicyNames = Array.Empty<string>();

    internal string cacheOutputPolicyName;

    public string BasePath { get; protected set; }

    internal bool disableRateLimiting;

    internal string rateLimitingPolicyName;

    /// <summary>
    /// Initializes a new instance of <see cref="AbstractEndpointModule"/>
    /// </summary>
    protected AbstractEndpointModule()
    {
        BasePath = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="AbstractEndpointModule"/>
    /// </summary>
    /// <param name="basePath">A base path to group routes in your <see cref="AbstractEndpointModule"/></param>
    protected AbstractEndpointModule(string basePath)
    {
        BasePath = basePath;
    }

    /// <summary>
    /// Add authorization to all routes
    /// </summary>
    /// <param name="policyNames">
    /// A collection of policy names.
    /// If <c>null</c> or empty, the default authorization policy will be used.
    /// </param>
    /// <returns></returns>
    public AbstractEndpointModule RequireAuthorization(params string[] policyNames)
    {
        requiresAuthorization = true;
        authorizationPolicyNames = policyNames;
        return this;
    }

    public abstract void AddRoutes(IEndpointRouteBuilder app);

    /// <summary>
    /// Requires that endpoints match one of the specified hosts during routing.
    /// </summary>
    /// <param name="hosts">The hosts used during routing</param>
    /// <returns></returns>
    public AbstractEndpointModule RequireHost(params string[] hosts)
    {
        this.hosts = hosts;
        return this;
    }

    /// <summary>
    /// Adds a CORS policy with the specified name to the module's routes.
    /// </summary>
    /// <param name="policyName">The CORS policy name</param>
    /// <returns></returns>
    public AbstractEndpointModule RequireCors(string policyName)
    {
        corsPolicyName = policyName;
        return this;
    }

    /// <summary>
    ///  Adds <see cref="IEndpointDescriptionMetadata"/> to <see cref="EndpointBuilder.Metadata"/> 
    /// </summary>
    /// <param name="description">The description value</param>
    /// <returns></returns>
    public AbstractEndpointModule WithDescription(string description)
    {
        openApiDescription = description;
        return this;
    }

    /// <summary>
    /// Adds the <see cref="IEndpointNameMetadata"/> to the Metadata collection for all endpoints produced
    /// </summary>
    /// <param name="name">The name value</param>
    /// <returns></returns>
    public AbstractEndpointModule WithName(string name)
    {
        openApiName = name;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="EndpointBuilder.DisplayName"/> to the provided <paramref name="displayName"/> for all routes in the module
    /// </summary>
    /// <param name="displayName">The display name value</param>
    /// <returns></returns>
    public AbstractEndpointModule WithDisplayName(string displayName)
    {
        openApiDisplayName = displayName;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="EndpointGroupNameAttribute"/> for all routes for all routes in the module
    /// </summary>
    /// <param name="groupName">The group name value</param>
    /// <returns></returns>
    public AbstractEndpointModule WithGroupName(string groupName)
    {
        openApiGroupName = groupName;
        return this;
    }

    /// <summary>
    /// Adds <see cref="IEndpointSummaryMetadata"/> to <see cref="EndpointBuilder.Metadata"/> for routes in the module
    /// </summary>
    /// <param name="summary">The summary value</param>
    /// <returns></returns>
    public AbstractEndpointModule WithSummary(string summary)
    {
        openApisummary = summary;
        return this;
    }

    /// <summary>
    /// Adds the provided metadata <paramref name="items"/> to <see cref="EndpointBuilder.Metadata"/> for all routes in the module
    /// </summary>
    /// <param name="items">The items to add</param>
    /// <returns></returns>
    public AbstractEndpointModule WithMetadata(params object[] items)
    {
        metaData = items;
        return this;
    }

    /// <summary>
    /// Adds the <see cref="ITagsMetadata"/> to <see cref="EndpointBuilder.Metadata"/> for all routes in the module
    /// </summary>
    /// <param name="tags">The tags to add</param>
    /// <returns></returns>
    public AbstractEndpointModule WithTags(params string[] tags)
    {
        this.tags = tags;
        return this;
    }

    /// <summary>
    /// Include all routes in the module to the OpenAPI output
    /// </summary>
    /// <returns></returns>
    public AbstractEndpointModule IncludeInOpenApi()
    {
        includeInOpenApi = true;
        return this;
    }

    /// <summary>
    ///  Marks an endpoint to be cached using a named policy.
    /// </summary>
    /// <param name="policyName">The policy name value</param>
    /// <returns></returns>
    public AbstractEndpointModule WithCacheOutput(string policyName)
    {
        cacheOutputPolicyName = policyName;
        return this;
    }

    /// <summary>
    /// Disables rate limiting on all the routes in the module
    /// </summary>
    /// <returns></returns>
    public AbstractEndpointModule DisableRateLimiting()
    {
        disableRateLimiting = true;
        return this;
    }

    /// <summary>
    /// Adds the specified rate limiting policy to all the routes in the module
    /// </summary>
    /// <param name="policyName">The policy name value</param>
    /// <returns></returns>
    public AbstractEndpointModule RequireRateLimiting(string policyName)
    {
        rateLimitingPolicyName = policyName;
        return this;
    }

    /// <summary>
    ///  Registers a filter given a delegate onto all routes in the module
    /// </summary>
    /// <remarks>
    ///  If a non null <see cref="IResult"/> is returned from the delegate, this will be returned and the delegate will not be executed
    /// </remarks>
    public Func<EndpointFilterInvocationContext, IResult> Before { get; set; }

    /// <summary>
    /// Registers a filter given a delegate onto all routes in the module
    /// </summary>
    public Action<EndpointFilterInvocationContext> After { get; set; }
}

/// <summary>
/// An interface to define HTTP routes
/// </summary>
public interface IEndpointModule
{
    /// <summary>
    /// Invoked at startup to add routes to the HTTP pipeline
    /// </summary>
    /// <param name="app">An instance of <see cref="IEndpointRouteBuilder"/></param>
    void AddRoutes(IEndpointRouteBuilder app);
}
