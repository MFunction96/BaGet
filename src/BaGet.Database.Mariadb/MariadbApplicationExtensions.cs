using BaGet.Core;
using BaGet.Database.Mariadb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BaGet
{
    public static class MariadbApplicationExtensions
    {
        public static BaGetApplication AddMariadbDatabase(this BaGetApplication app)
        {
            app.Services.AddBaGetDbContextProvider<MariadbContext>("Mariadb", (provider, options) =>
            {
                var databaseOptions = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>();

                options.UseMySql(databaseOptions.Value.ConnectionString, ServerVersion.AutoDetect(databaseOptions.Value.ConnectionString));
            });

            return app;
        }
    }
}
