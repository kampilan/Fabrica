using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Proxy.Appliance;

namespace Fabrica.Proxy
{

    class Program
    {

        static async Task Main(string[] args)
        {

            var bootstrap = new TheBootstrap();

            Console.WriteLine("Fabrica Proxy Appliance");
            Console.WriteLine("The Kampilan Group Inc. (c) 2022");
            Console.WriteLine("");

            await bootstrap.Run();

        }


    }


}
