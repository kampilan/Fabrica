using System;
using Fabrica.Api.Support.One;
using Fabrica.Watch.Api.Appliance;

Console.Clear();
Console.WriteLine("Fabrica Watch API Appliance");
Console.WriteLine("Pond Hawk Technologies Inc. (c) 2022");
Console.WriteLine("");

var app = await Appliance.Bootstrap<TheBootstrap>();

app.Run();

