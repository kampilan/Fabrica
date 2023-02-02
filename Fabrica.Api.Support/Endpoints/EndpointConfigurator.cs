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
using Fabrica.Api.Support.Endpoints.Negotiation;
using Microsoft.Extensions.Logging;

/// <summary>
/// Configures registrations of certain types within Carter
/// </summary>
public class EndpointConfigurator
{

    internal EndpointConfigurator()
    {
        ModuleTypes = new List<Type>();
        ValidatorTypes = new List<Type>();
        ResponseNegotiatorTypes = new List<Type>();
    }

    internal bool ExcludeValidators;

    internal bool ExcludeModules;

    internal bool ExcludeResponseNegotiators;

    internal List<Type> ModuleTypes { get; }

    internal List<Type> ValidatorTypes { get; }

    internal List<Type> ResponseNegotiatorTypes { get; }

    internal Type ModelBinder { get; set; }

    internal void LogDiscoveredCarterTypes(ILogger logger)
    {
        foreach (var validator in ValidatorTypes)
        {
            logger.LogDebug("Found validator {ValidatorName}", validator.Name);
        }

        foreach (var module in ModuleTypes)
        {
            logger.LogDebug("Found module {ModuleName}", module.FullName);
        }

        foreach (var negotiator in ResponseNegotiatorTypes)
        {
            logger.LogDebug("Found response negotiator {ResponseNegotiatorName}", negotiator.FullName);
        }
    }

    /// <summary>
    /// Register a specific <see cref="IEndpointModule"/>
    /// </summary>
    /// <typeparam name="TModule">The <see cref="IEndpointModule"/> to register</typeparam>
    /// <returns><see cref="EndpointConfigurator"/></returns>
    public EndpointConfigurator WithModule<TModule>() where TModule : IEndpointModule
    {
        ModuleTypes.Add(typeof(TModule));
        return this;
    }

    /// <summary>
    /// Register specific <see cref="AbstractEndpointModule"/>s
    /// </summary>
    /// <param name="modules">An array of <see cref="AbstractEndpointModule"/>s</param>
    /// <returns><see cref="EndpointConfigurator"/></returns>
    public EndpointConfigurator WithModules(params Type[] modules)
    {
        modules.MustDeriveFrom<IEndpointModule>();
        ModuleTypes.AddRange(modules);
        return this;
    }


    /// <summary>
    /// Register a specific <see cref="IResponseNegotiator"/>
    /// </summary>
    /// <typeparam name="T">The <see cref="IResponseNegotiator"/> to register</typeparam>
    /// <returns><see cref="EndpointConfigurator"/></returns>
    public EndpointConfigurator WithResponseNegotiator<T>() where T : IResponseNegotiator
    {
        ResponseNegotiatorTypes.Add(typeof(T));
        return this;
    }

    /// <summary>
    /// Register specific <see cref="IResponseNegotiator"/>s
    /// </summary>
    /// <param name="responseNegotiators">An array of <see cref="IResponseNegotiator"/>s</param>
    /// <returns><see cref="EndpointConfigurator"/></returns>
    public EndpointConfigurator WithResponseNegotiators(params Type[] responseNegotiators)
    {
        responseNegotiators.MustDeriveFrom<IResponseNegotiator>();
        ResponseNegotiatorTypes.AddRange(responseNegotiators);
        return this;
    }

    /// <summary>
    /// Do not register any validators
    /// </summary>
    /// <returns></returns>
    public EndpointConfigurator WithEmptyValidators()
    {
        ExcludeValidators = true;
        return this;
    }

    /// <summary>
    /// Do not register any modules
    /// </summary>
    /// <returns></returns>
    public EndpointConfigurator WithEmptyModules()
    {
        ExcludeModules = true;
        return this;
    }

    /// <summary>
    /// Do not register any response negotiators
    /// </summary>
    /// <returns></returns>
    public EndpointConfigurator WithEmptyResponseNegotiators()
    {
        ExcludeResponseNegotiators = true;
        return this;
    }
}