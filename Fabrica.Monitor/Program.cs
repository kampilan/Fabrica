using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Monitor.Appliance;

namespace Fabrica.Monitor
{


    class Program
    {
    
        static async Task Main(string[] args)
        {

            var bootstrap = new TheBootstrap();

            // *****************************************************************
            var headless = Console.OpenStandardOutput() == Stream.Null;
            if (!headless)
            {

                Console.Clear();
                Console.WriteLine("Fabrica Monitor Appliance");
                Console.WriteLine("The Kampilan Group Inc. (c) 2021");
                Console.WriteLine("");

            }

            await bootstrap.Run();

        }

    }
}
