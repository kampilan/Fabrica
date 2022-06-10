using System;
using Fabrica.Api.Support.One;
using Fabrica.Fake.Appliance;

Console.Clear();
Console.WriteLine("Fabrica Fake API Appliance");
Console.WriteLine("Pond Hawk Technologies Inc. (c) 2022");
Console.WriteLine("");

var app = await Appliance.Bootstrap<TheBootstrap>();

app.Run();

