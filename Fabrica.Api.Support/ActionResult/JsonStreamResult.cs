using System.IO;
using System.Threading.Tasks;
using Fabrica.Watch;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Support.ActionResult
{

    public class JsonStreamResult : Microsoft.AspNetCore.Mvc.ActionResult
    {

        public JsonStreamResult(MemoryStream content)
        {
            Content = content;
        }

        private MemoryStream Content { get; }


        public override async Task ExecuteResultAsync(ActionContext context)
        {

            using var logger = this.EnterMethod();


            await using( Content )
            {

                // *****************************************************************
                logger.Debug("Attempting to prepare response");
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = 200;


                // *****************************************************************
                logger.Debug("Attempting to write content to response stream");
                Content.Seek(0, SeekOrigin.Begin);
                await Content.CopyToAsync(context.HttpContext.Response.Body);

            }


        }

    }

}
