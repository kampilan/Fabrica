using System.Threading.Tasks;
using Fabrica.Api.Support.Controllers;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace Fabrica.Identity.Controller
{


    public class ResourceController: BaseController
    {


        public ResourceController(ICorrelation correlation, IOpenIddictApplicationManager manager) : base(correlation)
        {
            TheManager = manager;
        }

        private IOpenIddictApplicationManager TheManager { get; }


        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("message")]
        public async Task<IActionResult> GetMessage()
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to find Subject ");
            var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;

            logger.Inspect(nameof(subject), subject);

            if( string.IsNullOrEmpty(subject) )
                return BadRequest();



            // *****************************************************************
            logger.Debug("Attempting to Application for Subject");
            var application = await TheManager.FindByClientIdAsync(subject);

            logger.Inspect(nameof(application), application);

            if (application == null)
                return BadRequest();



            // *****************************************************************
            logger.Debug("Attempting to get User DisplayName");
            var displayName = await TheManager.GetDisplayNameAsync(application);

            logger.Inspect(nameof(displayName), displayName);



            // *****************************************************************
            return Content( $"{displayName} has been successfully authenticated." );


        }


    }


}
