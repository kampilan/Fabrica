﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Fake.Appliance;

namespace Fabrica.Fake
{

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
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
                Console.WriteLine("Fabrica Fake API Appliance");
                Console.WriteLine("The Kampilan Group Inc. (c) 2021");
                Console.WriteLine("");

            }

            await bootstrap.Run();

        }

    }

}
