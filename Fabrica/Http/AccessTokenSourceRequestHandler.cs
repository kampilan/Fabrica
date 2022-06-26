// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Identity;
using Fabrica.Watch;

namespace Fabrica.Http;

public class AccessTokenSourceRequestHandler: DelegatingHandler
{

    public AccessTokenSourceRequestHandler( IEnumerable<IAccessTokenSource> sources )
    {
        Sources = sources.ToList();
    }

    public string Name { get; set; } = "Api";
    private List<IAccessTokenSource> Sources { get; }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();

        logger.Inspect(nameof(Name), Name);
        logger.Inspect(nameof(Sources.Count), Sources.Count);



        // *****************************************************************
        logger.Debug("Attempting to find Token Source");
        var source = Sources.FirstOrDefault(s => s.Name == Name) ?? Sources.FirstOrDefault();
        if( source is null )
            throw new InvalidOperationException("No Access Token Sources are registered");



        // *****************************************************************
        logger.Debug("Attempting to get current access token");
        var token = await source.GetToken();

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Null or blank token returned by AccessToken Source");

        logger.Inspect(nameof(token.Length), token.Length);



        // *****************************************************************
        logger.Debug("Attempting to add Authorization header");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);



        // *****************************************************************
        logger.Debug("Attempting to Send request");
        var response = await base.SendAsync(request, cancellationToken);



        // *****************************************************************
        return response;


    }

}