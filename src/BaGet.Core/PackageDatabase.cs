using BaGet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Core
{
    public class PackageDatabase(BaGetDbContext context, ILogger<PackageDatabase> logger) : IPackageDatabase
    {
        public async Task<PackageAddResult> AddAsync(Package package, CancellationToken cancellationToken)
        {
            try
            {
                context.Packages.Add(package);

                await context.SaveChangesAsync(cancellationToken);

                return PackageAddResult.Success;
            }
            catch (DbUpdateException e)
            {
                logger.LogWarning(e, "Failed to add package metadata to Database. {package}", JsonConvert.SerializeObject(package, Formatting.None));
                return PackageAddResult.PackageAlreadyExists;
            }
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
        {
            return await context
                .Packages
                .Where(p => p.Id == id)
                .AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            return await context
                .Packages
                .Where(p => p.Id == id)
                .Where(p => p.NormalizedVersionString == version.ToNormalizedString())
                .AnyAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Package>> FindAsync(string id, bool includeUnlisted, CancellationToken cancellationToken)
        {
            var query = context.Packages
                .Include(p => p.Dependencies)
                .Include(p => p.PackageTypes)
                .Include(p => p.TargetFrameworks)
                .Where(p => p.Id == id);

            if (!includeUnlisted)
            {
                query = query.Where(p => p.Listed);
            }

            return (await query.ToListAsync(cancellationToken)).AsReadOnly();
        }

        public Task<Package?> FindOrNullAsync(
            string id,
            NuGetVersion version,
            bool includeUnlisted,
            CancellationToken cancellationToken)
        {
            var query = context.Packages
                .Include(p => p.Dependencies)
                .Include(p => p.TargetFrameworks)
                .Where(p => p.Id == id)
                .Where(p => p.NormalizedVersionString == version.ToNormalizedString());

            if (!includeUnlisted)
            {
                query = query.Where(p => p.Listed);
            }

            return query.FirstOrDefaultAsync(cancellationToken);
        }

        public Task<bool> UnlistPackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            return TryUpdatePackageAsync(id, version, p => p.Listed = false, cancellationToken);
        }

        public Task<bool> RelistPackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            return TryUpdatePackageAsync(id, version, p => p.Listed = true, cancellationToken);
        }

        public async Task AddDownloadAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            await TryUpdatePackageAsync(id, version, p => p.Downloads += 1, cancellationToken);
        }

        public async Task<bool> HardDeletePackageAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            var package = await context.Packages
                .Where(p => p.Id == id)
                .Where(p => p.NormalizedVersionString == version.ToNormalizedString())
                .Include(p => p.Dependencies)
                .Include(p => p.TargetFrameworks)
                .FirstOrDefaultAsync(cancellationToken);

            if (package == null)
            {
                return false;
            }

            context.Packages.Remove(package);
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }

        private async Task<bool> TryUpdatePackageAsync(
            string id,
            NuGetVersion version,
            Action<Package> action,
            CancellationToken cancellationToken)
        {
            var package = await context.Packages
                .Where(p => p.Id == id)
                .Where(p => p.NormalizedVersionString == version.ToNormalizedString())
                .FirstOrDefaultAsync();

            if (package != null)
            {
                action(package);
                await context.SaveChangesAsync(cancellationToken);

                return true;
            }

            return false;
        }
    }
}
