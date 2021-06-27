using System;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Identity.Contexts;
using Fabrica.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace Fabrica.Identity.Services
{

    public class Worker: IHostedService
    {

        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            if (await manager.FindByClientIdAsync("console", cancellationToken) == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId     = "console",
                    ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207",
                    DisplayName  = "My client application",
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddictConstants.Permissions.GrantTypes.Password
                    }
                }, cancellationToken);
            }

            var um = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            if( await um.FindByLoginAsync("", "admin") == null )
            {

                var user = new ApplicationUser
                {
                    UserName       = "admin",
                    Email          = "admin@kampilangroup.com",
                    EmailConfirmed = true,
                };

                await um.CreateAsync(user, "RumbleMonkey!2009");

            }



        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


    }


}
