﻿// ReSharper disable UnusedMember.Global

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Watch;

namespace Fabrica.Identity;

public class AccessTokenSourceRequestHandler: DelegatingHandler
{

    public AccessTokenSourceRequestHandler( IAccessTokenSource source )
    {
        Source = source;
    }

    private IAccessTokenSource Source { get; }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();

        logger.Inspect(nameof(Source.HasExpired), Source.HasExpired);



        // *****************************************************************
        logger.Debug("Attempting to get current access token");
        var token = await Source.GetToken();

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