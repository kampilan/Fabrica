using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Fabrica.Make.Sdk.Models;
using Fabrica.Watch;

namespace Fabrica.Make.Sdk;

public static class MakeClient
{


    public static async Task<ScenarioResponse?> GetScenarios(this IHttpClientFactory factory, int teamId)
    {

        using var logger = WatchFactoryLocator.Factory.GetLogger(typeof(MakeClient));
        logger.EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to get Client from factory");
        using var client = factory.CreateClient("MakeApi");



        // *****************************************************************
        logger.Debug("Attempting to Get");
        var uri = new Uri($"scenarios?teamId={teamId}", UriKind.Relative);
        var response = await client.GetAsync(uri);

        logger.Inspect(nameof(response.StatusCode), response.StatusCode);

        response.EnsureSuccessStatusCode();



        // *****************************************************************
        logger.Debug("Attempting to read response contents");
        var json = await response.Content.ReadAsStringAsync();
        logger.LogJson("GetScenarios json", json );




        // *****************************************************************
        logger.Debug("Attempting to parse response");
        var scenarios = JsonSerializer.Deserialize<ScenarioResponse>(json);



        // *****************************************************************
        return scenarios;

    }


    public static async Task<HookResponse?> GetHooks(this IHttpClientFactory factory, int teamId)
    {

        using var logger = WatchFactoryLocator.Factory.GetLogger( typeof(MakeClient) );
        logger.EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to get Client from factory");
        using var client = factory.CreateClient("MakeApi");



        // *****************************************************************
        logger.Debug("Attempting to Get");
        var uri = new Uri($"hooks?teamId={teamId}", UriKind.Relative);
        var response = await client.GetAsync(uri);

        logger.Inspect(nameof(response.StatusCode), response.StatusCode);

        response.EnsureSuccessStatusCode();



        // *****************************************************************
        logger.Debug("Attempting to read response contents");
        var json = await response.Content.ReadAsStringAsync();
        logger.LogJson("GetHooks json", json);



        // *****************************************************************
        logger.Debug("Attempting to parse response");
        var hooks = JsonSerializer.Deserialize<HookResponse>(json);



        // *****************************************************************
        return hooks;

    }


}