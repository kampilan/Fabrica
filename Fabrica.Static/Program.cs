using System;
using Fabrica.Api.Support.One;
using Fabrica.Static.Appliance;

Console.Clear();
Console.WriteLine("Fabrica Static Appliance");
Console.WriteLine("Pond Hawk Technologies Inc. (c) 2022");
Console.WriteLine("");

var app = await Appliance.Bootstrap<TheBootstrap>();

app.Run();

