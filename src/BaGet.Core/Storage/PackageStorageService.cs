using BaGet.Core.Entities;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace BaGet.Core.Storage
{
    public class PackageStorageService(
        IStorageService storage,
        ILogger<PackageStorageService> logger)
        : IPackageStorageService
    {
        private const string PackagesPathPrefix = "packages";

        // See: https://github.com/NuGet/NuGetGallery/blob/73a5c54629056b25b3a59960373e8fef88abff36/src/NuGetGallery.Core/CoreConstants.cs#L19
        private const string PackageContentType = "binary/octet-stream";
        private const string NuspecContentType = "text/plain";
        private const string ReadmeContentType = "text/markdown";
        private const string IconContentType = "image/xyz";

        public async Task SavePackageContentAsync(
            Package package,
            Stream packageStream,
            FileCache nuspecCacheFile,
            FileCache? readmeCacheFile,
            FileCache? iconCacheFile,
            CancellationToken cancellationToken = default)
        {
            var lowercasedId = package.Id.ToLowerInvariant();
            var lowercasedNormalizedVersion = package.NormalizedVersionString.ToLowerInvariant();

            var packagePath = PackagePath(lowercasedId, lowercasedNormalizedVersion);
            var nuspecPath = NuspecPath(lowercasedId, lowercasedNormalizedVersion);
            var readmePath = ReadmePath(lowercasedId, lowercasedNormalizedVersion);
            var iconPath = IconPath(lowercasedId, lowercasedNormalizedVersion);

            logger.LogInformation(
                "Storing package {PackageId} {PackageVersion} at {Path}...",
                lowercasedId,
                lowercasedNormalizedVersion,
                packagePath);

            // Store the package.
            var result = await storage.PutAsync(packagePath, packageStream, PackageContentType, cancellationToken);
            if (result == StoragePutResult.Conflict)
            {
                // TODO: This should be returned gracefully with an enum.
                logger.LogInformation(
                    "Could not store package {PackageId} {PackageVersion} at {Path} due to conflict",
                    lowercasedId,
                    lowercasedNormalizedVersion,
                    packagePath);

                throw new InvalidOperationException($"Failed to store package {lowercasedId} {lowercasedNormalizedVersion} due to conflict");
            }

            // Store the package's nuspec.
            logger.LogInformation(
                "Storing package {PackageId} {PackageVersion} nuspec at {Path}...",
                lowercasedId,
                lowercasedNormalizedVersion,
                nuspecPath);

            await using var nuspecStream = new BufferedStream(new FileStream(nuspecCacheFile.FullPath, FileMode.Open, FileAccess.Read));
            result = await storage.PutAsync(nuspecPath, nuspecStream, NuspecContentType, cancellationToken);
            if (result == StoragePutResult.Conflict)
            {
                // TODO: This should be returned gracefully with an enum.
                logger.LogInformation(
                    "Could not store package {PackageId} {PackageVersion} nuspec at {Path} due to conflict",
                    lowercasedId,
                    lowercasedNormalizedVersion,
                    nuspecPath);

                throw new InvalidOperationException($"Failed to store package {lowercasedId} {lowercasedNormalizedVersion} nuspec due to conflict");
            }

            // Store the package's readme, if one exists.
            if (readmeCacheFile is not null)
            {
                logger.LogInformation(
                    "Storing package {PackageId} {PackageVersion} readme at {Path}...",
                    lowercasedId,
                    lowercasedNormalizedVersion,
                    readmePath);

                await using var readmeStream = new BufferedStream(new FileStream(readmeCacheFile.FullPath, FileMode.Open, FileAccess.Read));
                result = await storage.PutAsync(readmePath, readmeStream, ReadmeContentType, cancellationToken);
                if (result == StoragePutResult.Conflict)
                {
                    // TODO: This should be returned gracefully with an enum.
                    logger.LogInformation(
                        "Could not store package {PackageId} {PackageVersion} readme at {Path} due to conflict",
                        lowercasedId,
                        lowercasedNormalizedVersion,
                        readmePath);

                    throw new InvalidOperationException($"Failed to store package {lowercasedId} {lowercasedNormalizedVersion} readme due to conflict");
                }
            }

            // Store the package's icon, if one exists.
            if (iconCacheFile is not null)
            {
                logger.LogInformation(
                    "Storing package {PackageId} {PackageVersion} icon at {Path}...",
                    lowercasedId,
                    lowercasedNormalizedVersion,
                    iconPath);

                await using var iconStream = new BufferedStream(new FileStream(iconCacheFile.FullPath, FileMode.Open, FileAccess.Read));
                result = await storage.PutAsync(iconPath, iconStream, IconContentType, cancellationToken);
                if (result == StoragePutResult.Conflict)
                {
                    // TODO: This should be returned gracefully with an enum.
                    logger.LogInformation(
                        "Could not store package {PackageId} {PackageVersion} icon at {Path} due to conflict",
                        lowercasedId,
                        lowercasedNormalizedVersion,
                        iconPath);

                    throw new InvalidOperationException($"Failed to store package {lowercasedId} {lowercasedNormalizedVersion} icon");
                }
            }

            logger.LogInformation(
                "Finished storing package {PackageId} {PackageVersion}",
                lowercasedId,
                lowercasedNormalizedVersion);
        }

        public async Task<Stream> GetPackageStreamAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            return await GetStreamAsync(id, version, PackagePath, cancellationToken);
        }

        public async Task<Stream> GetNuspecStreamAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            return await GetStreamAsync(id, version, NuspecPath, cancellationToken);
        }

        public async Task<Stream> GetReadmeStreamAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            return await GetStreamAsync(id, version, ReadmePath, cancellationToken);
        }

        public async Task<Stream> GetIconStreamAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            return await GetStreamAsync(id, version, IconPath, cancellationToken);
        }

        public async Task DeleteAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
        {
            var lowercasedId = id.ToLowerInvariant();
            var lowercasedNormalizedVersion = version.ToNormalizedString().ToLowerInvariant();

            var packagePath = PackagePath(lowercasedId, lowercasedNormalizedVersion);
            var nuspecPath = NuspecPath(lowercasedId, lowercasedNormalizedVersion);
            var readmePath = ReadmePath(lowercasedId, lowercasedNormalizedVersion);
            var iconPath = IconPath(lowercasedId, lowercasedNormalizedVersion);

            await storage.DeleteAsync(packagePath, cancellationToken);
            await storage.DeleteAsync(nuspecPath, cancellationToken);
            await storage.DeleteAsync(readmePath, cancellationToken);
            await storage.DeleteAsync(iconPath, cancellationToken);
        }

        private async Task<Stream> GetStreamAsync(
            string id,
            NuGetVersion version,
            Func<string, string, string> pathFunc,
            CancellationToken cancellationToken)
        {
            var lowercasedId = id.ToLowerInvariant();
            var lowercasedNormalizedVersion = version.ToNormalizedString().ToLowerInvariant();
            var path = pathFunc(lowercasedId, lowercasedNormalizedVersion);

            try
            {
                return await storage.GetAsync(path, cancellationToken);
            }
            catch (DirectoryNotFoundException)
            {
                // The "packages" prefix was lowercased, which was a breaking change
                // on filesystems that are case sensitive. Handle this case to help
                // users migrate to the latest version of BaGet.
                // See https://github.com/loic-sharma/BaGet/issues/298
                logger.LogError(
                    $"Unable to find the '{PackagesPathPrefix}' folder. " +
                    "If you've recently upgraded BaGet, please make sure this folder starts with a lowercased letter. " +
                    "For more information, please see https://github.com/loic-sharma/BaGet/issues/298");
                throw;
            }
        }

        private string PackagePath(string lowercasedId, string lowercasedNormalizedVersion)
        {
            return Path.Combine(
                PackagesPathPrefix,
                lowercasedId,
                lowercasedNormalizedVersion,
                $"{lowercasedId}.{lowercasedNormalizedVersion}.nupkg");
        }

        private string NuspecPath(string lowercasedId, string lowercasedNormalizedVersion)
        {
            return Path.Combine(
                PackagesPathPrefix,
                lowercasedId,
                lowercasedNormalizedVersion,
                $"{lowercasedId}.nuspec");
        }

        private string ReadmePath(string lowercasedId, string lowercasedNormalizedVersion)
        {
            return Path.Combine(
                PackagesPathPrefix,
                lowercasedId,
                lowercasedNormalizedVersion,
                "readme");
        }

        private string IconPath(string lowercasedId, string lowercasedNormalizedVersion)
        {
            return Path.Combine(
                PackagesPathPrefix,
                lowercasedId,
                lowercasedNormalizedVersion,
                "icon");
        }
    }
}
