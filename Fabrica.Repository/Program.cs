using Fabrica.Api.Support.One;
using Fabrica.Repository.Appliance;

var app = await Appliance.Bootstrap<TheBootstrap>();

Console.WriteLine("Fabrica Repository Appliance");
Console.WriteLine("Pond Hawk Technologies Inc. (c) 2022");
Console.WriteLine("");

app.Run();