using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Static.Appliance;

namespace Fabrica.Static
{


    class Program
    {


        // ReSharper disable once UnusedParameter.Local
        static async Task Main(string[] args)
        {


            // *****************************************************************
            var headless = Console.OpenStandardOutput() == Stream.Null;
            if( !headless )
            {

                Console.Clear();
                Console.WriteLine("Fabrica Static Appliance");
                Console.WriteLine("The Kampilan Group Inc. (c) 2020");
                Console.WriteLine("");

            }


            // *****************************************************************
            var bootstrap = new TheBootstrap();

            await bootstrap.Run();

        }


    }



}
