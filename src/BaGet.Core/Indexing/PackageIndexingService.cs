using BaGet.Core.Configuration;
using BaGet.Core.Entities;
using BaGet.Core.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace BaGet.Core.Indexing
{
    public class PackageIndexingService(
        IPackageDatabase packages,
        IPackageStorageService storage,
        IOptions<BaGetOptions> options,
        IFileCachePool fileCachePool,
        ILogger<PackageIndexingService> logger)
        : IPackageIndexingService
    {
        public async Task<PackageIndexingResult> IndexAsync(Stream packageStream, CancellationToken cancellationToken)
        {
            // Try to extract all the necessary information from the package.
            var nuspecFile = fileCachePool.Register();
            var readmeFile = fileCachePool.Register();
            var iconFile = fileCachePool.Register();

            Package package;
            
            try
            {
                var packageReader = new PackageArchiveReader(packageStream, leaveStreamOpen: true);
                package = packageReader.GetPackageMetadata();
                package.Published = DateTime.Now;

                var receivedTasks = new List<Task>
                {
                    Task.Run(async () =>
                    {
                        await using var nuspecFileStream =
                            new BufferedStream(new FileStream(nuspecFile.FullPath, FileMode.Create, FileAccess.Write));
                        await using var nuspecStream = await packageReader.GetNuspecAsync(cancellationToken);
                        await nuspecStream.CopyToAsync(nuspecFileStream, cancellationToken);

                    }, cancellationToken)
                };

                if (package.HasReadme)
                {
                    receivedTasks.Add(Task.Run(async () =>
                    {
                        await using var readmeFileStream =
                            new BufferedStream(new FileStream(readmeFile.FullPath, FileMode.Create, FileAccess.Write));
                        await using var nuspecFileStream =
                            new BufferedStream(new FileStream(nuspecFile.FullPath, FileMode.Create, FileAccess.Write));
                        await using var readmeStream = await packageReader.GetReadmeAsync(cancellationToken);
                        receivedTasks.Add(readmeStream.CopyToAsync(readmeFileStream, cancellationToken));

                    }, cancellationToken));
                    
                }

                if (package.HasEmbeddedIcon)
                {
                    receivedTasks.Add(Task.Run(async () =>
                    {
                        await using var iconFileStream =
                            new BufferedStream(new FileStream(iconFile.FullPath, FileMode.Create, FileAccess.Write));
                        await using var nuspecFileStream =
                            new BufferedStream(new FileStream(nuspecFile.FullPath, FileMode.Create, FileAccess.Write));
                        await using var iconStream = await packageReader.GetIconAsync(cancellationToken);
                        receivedTasks.Add(iconStream.CopyToAsync(iconFileStream, cancellationToken));

                    }, cancellationToken));
                    
                }

                Task.WaitAll(receivedTasks.ToArray(), cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Uploaded package is invalid");
                fileCachePool.UnRegister(nuspecFile);
                fileCachePool.UnRegister(readmeFile);
                fileCachePool.UnRegister(iconFile);
                return PackageIndexingResult.InvalidPackage;
            }

            // The package is well-formed. Ensure this is a new package.
            if (await packages.ExistsAsync(package.Id, package.Version, cancellationToken))
            {
                if (!options.Value.AllowPackageOverwrites)
                {
                    fileCachePool.UnRegister(nuspecFile);
                    fileCachePool.UnRegister(readmeFile);
                    fileCachePool.UnRegister(iconFile);
                    return PackageIndexingResult.PackageAlreadyExists;
                }

                await packages.HardDeletePackageAsync(package.Id, package.Version, cancellationToken);
                await storage.DeleteAsync(package.Id, package.Version, cancellationToken);
            }

            // TODO: Add more package validations
            // TODO: Call PackageArchiveReader.ValidatePackageEntriesAsync
            logger.LogInformation(
                "Validated package {PackageId} {PackageVersion}, persisting content to storage...",
                package.Id,
                package.NormalizedVersionString);

            try
            {
                packageStream.Position = 0;

                await storage.SavePackageContentAsync(
                    package,
                    packageStream,
                    nuspecFile,
                    readmeFile,
                    iconFile,
                    cancellationToken);
            }
            catch (Exception e)
            {
                // This may happen due to concurrent pushes.
                // TODO: Make IPackageStorageService.SavePackageContentAsync return a result enum so this
                // can be properly handled.
                logger.LogError(
                    e,
                    "Failed to persist package {PackageId} {PackageVersion} content to storage",
                    package.Id,
                    package.NormalizedVersionString);

                throw;
            }
            finally
            {
                fileCachePool.UnRegister(nuspecFile);
                fileCachePool.UnRegister(readmeFile);
                fileCachePool.UnRegister(iconFile);
            }

            logger.LogInformation(
                "Persisted package {Id} {Version} content to storage, saving metadata to database...",
                package.Id,
                package.NormalizedVersionString);

            var result = await packages.AddAsync(package, cancellationToken);
            if (result == PackageAddResult.PackageAlreadyExists)
            {
                logger.LogWarning(
                    "Package {Id} {Version} metadata already exists in database",
                    package.Id,
                    package.NormalizedVersionString);

                return PackageIndexingResult.PackageAlreadyExists;
            }

            if (result != PackageAddResult.Success)
            {
                logger.LogError($"Unknown {nameof(PackageAddResult)} value: {{PackageAddResult}}", result);

                throw new InvalidOperationException($"Unknown {nameof(PackageAddResult)} value: {result}");
            }

            logger.LogInformation(
                "Successfully persisted package {Id} {Version} metadata to database. Indexing in search...",
                package.Id,
                package.NormalizedVersionString);

            return PackageIndexingResult.Success;
        }
    }
}
