/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

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

// ReSharper disable UnusedMember.Global

using Fabrica.Watch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;


namespace Fabrica.Api.Support.Conventions;

public class DefaultAuthorizeConvention<TAuthorizationFilter> : IApplicationModelConvention where TAuthorizationFilter : class, IAuthorizationFilter, new()
{

    public void Apply( ApplicationModel application )
    {

        using var logger = this.EnterMethod();


        logger.Inspect("AuthorizationFilter type", typeof(TAuthorizationFilter).FullName);


        foreach (var controller in application.Controllers)
        {


            logger.Inspect(nameof(controller.ControllerName), controller.ControllerName);


            // *****************************************************************
            logger.Debug("Attempting to check for Authorize attributes");
            var attributes = controller.ControllerType.GetCustomAttributes(typeof(AuthorizeAttribute), false);

            if (logger.IsDebugEnabled)
            {
                logger.Inspect(nameof(attributes.Length), attributes.Length);
                var policies = attributes.Cast<AuthorizeAttribute>().Select(a => a.Policy).ToList();
                logger.LogObject(nameof(policies), policies);
            }

            if (attributes.Length > 0)
                continue;


            controller.Filters.Add(new TAuthorizationFilter());

        }


    }


}