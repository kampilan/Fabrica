using Fabrica.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Core.Connections;

namespace Fabrica.Identity.Contexts
{

    public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext()
        {

        }

        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : base(options)
        {
        }


        protected override void OnConfiguring( DbContextOptionsBuilder builder )
        {

            var cnstr = "Server=10.78.142.10; Database=fabrica_identity;Uid=appserver;Pwd=rHUAZWHe9rDrRedL8hcT;UseAffectedRows=false;AllowUserVariables=true";
            builder.UseMySql(cnstr, ServerVersion.Parse(cnstr));

        }


        protected override void OnModelCreating(ModelBuilder builder )
        {

            base.OnModelCreating( builder );

            builder.UseOpenIddict();

        }


    }


}
