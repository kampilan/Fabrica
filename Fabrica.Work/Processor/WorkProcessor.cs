/*
The MIT License (MIT)

Copyright (c) 2021 The Kampilan Group Inc.

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

using System.Net.Http;
using System.Threading.Tasks;
using Fabrica.Identity;
using Fabrica.Watch;
using Fabrica.Work.Persistence.Contexts;
using Newtonsoft.Json;

namespace Fabrica.Work.Processor
{


    public class WorkProcessor : AbstractWorkProcessor
    {


        public WorkProcessor( IHttpClientFactory factory, WorkDbContext context, IAccessTokenSource tokenSource ): base(factory, context, tokenSource)
        {
        }


        protected override Task<(bool proceed, string payload)> SerializePayload(WorkRequest request)
        {

            using var logger = this.EnterMethod();

            var json = request.Payload.ToString(Formatting.None);

            return Task.FromResult((true,json));

        }

        protected override Task Accepted(WorkRequest request)
        {
            return Task.CompletedTask;
        }

        protected override Task Rejected(WorkRequest request)
        {
            return Task.CompletedTask;
        }


    }


}
