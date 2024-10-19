using BaGet.Protocol.Models;
using NuGet.Versioning;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core.Storage;

namespace BaGet.Core.Content
{
    /// <summary>
    /// Implements the NuGet Package Content resource in NuGet's V3 protocol.
    /// </summary>
    public class DefaultPackageContentService(
        IPackageService packages,
        IPackageStorageService storage)
        : IPackageContentService
    {
        public async Task<PackageVersionsResponse?> GetPackageVersionsOrNullAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            var versions = await packages.FindPackageVersionsAsync(id, cancellationToken);
            if (!versions.Any())
            {
                return null;
            }

            return new PackageVersionsResponse
            {
                Versions = versions
                    .Select(v => v.ToNormalizedString())
                    .Select(v => v.ToLowerInvariant())
                    .ToList()
            };
        }

        public async Task<Stream?> GetPackageContentStreamOrNullAsync(
            string id,
            NuGetVersion version,
            CancellationToken cancellationToken = default)
        {
            if (!await packages.ExistsAsync(id, version, cancellationToken))
            {
                return null;
            }

            await packages.AddDownloadAsync(id, version, cancellationToken);
            return await storage.GetPackageStreamAsync(id, version, cancellationToken);
        }

        public async Task<Stream?> GetPackageManifestStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken = default)
        {
            if (!await packages.ExistsAsync(id, version, cancellationToken))
            {
                return null;
            }

            return await storage.GetNuspecStreamAsync(id, version, cancellationToken);
        }

        public async Task<Stream?> GetPackageReadmeStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken = default)
        {
            var package = await packages.FindPackageOrNullAsync(id, version, cancellationToken);
            if (package is not { HasReadme: true })
            {
                return null;
            }

            return await storage.GetReadmeStreamAsync(id, version, cancellationToken);
        }

        public async Task<Stream?> GetPackageIconStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken = default)
        {
            var package = await packages.FindPackageOrNullAsync(id, version, cancellationToken);
            if (package is not { HasEmbeddedIcon: true })
            {
                return null;
            }

            return await storage.GetIconStreamAsync(id, version, cancellationToken);
        }
    }
}
