using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Carly.Configuration;
using Carly.Web;

namespace Carly.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class CarlyDbContextFactory : IDesignTimeDbContextFactory<CarlyDbContext>
    {
        public CarlyDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CarlyDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            CarlyDbContextConfigurer.Configure(builder, configuration.GetConnectionString(CarlyConsts.ConnectionStringName));

            return new CarlyDbContext(builder.Options);
        }
    }
}
