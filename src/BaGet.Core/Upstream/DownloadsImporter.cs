using BaGet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Core.Upstream
{
    public class DownloadsImporter(
        BaGetDbContext context,
        IPackageDownloadsSource downloadsSource,
        ILogger<DownloadsImporter> logger)
    {
        private const int BatchSize = 200;

        public async Task ImportAsync(CancellationToken cancellationToken)
        {
            var packageDownloads = await downloadsSource.GetPackageDownloadsAsync();
            var packages = await context.Packages.CountAsync();
            var batches = (packages / BatchSize) + 1;

            for (var batch = 0; batch < batches; batch++)
            {
                logger.LogInformation("Importing batch {Batch}...", batch);

                foreach (var package in await GetBatchAsync(batch, cancellationToken))
                {
                    var packageId = package.Id.ToLowerInvariant();
                    var packageVersion = package.NormalizedVersionString.ToLowerInvariant();

                    if (!packageDownloads.ContainsKey(packageId) ||
                        !packageDownloads[packageId].ContainsKey(packageVersion))
                    {
                        continue;
                    }

                    package.Downloads = packageDownloads[packageId][packageVersion];
                }

                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Imported batch {Batch}", batch);
            }
        }

        private Task<List<Package>> GetBatchAsync(int batch, CancellationToken cancellationToken)
            => context.Packages
                .OrderBy(p => p.Key)
                .Skip(batch * BatchSize)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);
    }
}
