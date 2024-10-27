using BaGet.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Tests.Support
{
    public class BaGetApplication
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Create temporary storage paths.
            var tempPath = Path.Combine(
                Path.GetTempPath(),
                "BaGetTests",
                Guid.NewGuid().ToString("N"));
            var sqlitePath = Path.Combine(tempPath, "BaGet.db");
            var storagePath = Path.Combine(tempPath, "Packages");

            Directory.CreateDirectory(tempPath);

            builder
                .UseEnvironment("Production")
                //.ConfigureLogging(logging =>
                //{
                //    // BaGet uses console logging by default. This logger throws operation
                //    // cancelled exceptions when the host shuts down, causing the the debugger
                //    // to pause repeatedly if CLR exceptions are enabled.
                //    logging.ClearProviders();

                //    // Pipe logs to the xunit output.
                //    logging.AddProvider(new XunitLoggerProvider(output));
                //})
                .ConfigureAppConfiguration(config =>
                {
                    // Setup the integration test configuration.
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "Database:Type", "Sqlite" },
                        { "Database:ConnectionString", $"Data Source={sqlitePath}" },
                        { "Storage:Type", "FileSystem" },
                        { "Storage:Path", storagePath },
                        { "Search:Type", "Database" }
                    });
                });
        }
    }

    internal static class BaGetWebApplicationFactoryExtensions
    {
        public static async Task AddPackageAsync(
            this WebApplicationFactory<Program> factory,
            Stream package,
            CancellationToken cancellationToken = default)
        {
            var scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

            using var scope = scopeFactory.CreateScope();
            var indexer = scope.ServiceProvider.GetRequiredService<IPackageIndexingService>();

            var result = await indexer.IndexAsync(package, cancellationToken);
            if (result != PackageIndexingResult.Success)
            {
                throw new InvalidOperationException($"Unexpected indexing result {result}");
            }
        }

        public static async Task AddSymbolPackageAsync(
            this WebApplicationFactory<Program> factory,
            Stream symbolPackage,
            CancellationToken cancellationToken = default)
        {
            var scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

            using var scope = scopeFactory.CreateScope();
            var indexer = scope.ServiceProvider.GetRequiredService<ISymbolIndexingService>();

            var result = await indexer.IndexAsync(symbolPackage, cancellationToken);
            if (result != SymbolIndexingResult.Success)
            {
                throw new InvalidOperationException($"Unexpected indexing result {result}");
            }
        }
    }
}
