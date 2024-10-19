using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace BaGet.Core.Indexing
{
    // Based off: https://github.com/NuGet/NuGetGallery/blob/master/src/NuGetGallery/Services/SymbolPackageUploadService.cs
    // Based off: https://github.com/NuGet/NuGet.Jobs/blob/master/src/Validation.Symbols/SymbolsValidatorService.cs#L44
    public class SymbolIndexingService(
        IPackageDatabase packages,
        ISymbolStorageService storage,
        IFileCachePool fileCachePool,
        ILogger<SymbolIndexingService> logger)
        : ISymbolIndexingService
    {
        private static readonly HashSet<string> ValidSymbolPackageContentExtensions =
        [
            ".pdb",
            ".nuspec",
            ".xml",
            ".psmdcp",
            ".rels",
            ".p7s"
        ];

        public async Task<SymbolIndexingResult> IndexAsync(Stream stream, CancellationToken cancellationToken)
        {
            try
            {
                using var symbolPackage = new PackageArchiveReader(stream, leaveStreamOpen: true);
                var pdbPaths = await GetSymbolPackagePdbPathsOrNullAsync(symbolPackage, cancellationToken);
                if (pdbPaths == null)
                {
                    return SymbolIndexingResult.InvalidSymbolPackage;
                }

                // Ensure a corresponding NuGet package exists.
                var packageId = symbolPackage.NuspecReader.GetId();
                var packageVersion = symbolPackage.NuspecReader.GetVersion();

                var package = await packages.FindOrNullAsync(packageId, packageVersion, includeUnlisted: true, cancellationToken);
                if (package is null)
                {
                    return SymbolIndexingResult.PackageNotFound;
                }

                using var pdbs = new PdbList();
                // Extract the portable PDBs from the snupkg. Nothing is persisted until after all
                // PDBs have been extracted and validated sucessfully.
                foreach (var pdbPath in pdbPaths)
                {
                    var portablePdb = await ExtractPortablePdbAsync(symbolPackage, pdbPath, cancellationToken);
                    if (portablePdb == null)
                    {
                        return SymbolIndexingResult.InvalidSymbolPackage;
                    }

                    pdbs.Add(portablePdb);
                }

                // Persist the portable PDBs to storage.
                foreach (var pdb in pdbs)
                {
                    await storage.SavePortablePdbContentAsync(pdb.Filename, pdb.Key, pdb.Content, cancellationToken);
                }

                return SymbolIndexingResult.Success;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to index symbol package due to exception");
                return SymbolIndexingResult.InvalidSymbolPackage;
            }
        }

        private async Task<IReadOnlyList<string>?> GetSymbolPackagePdbPathsOrNullAsync(
            PackageArchiveReader symbolPackage,
            CancellationToken cancellationToken)
        {
            try
            {
                await symbolPackage.ValidatePackageEntriesAsync(cancellationToken);

                var files = (await symbolPackage.GetFilesAsync(cancellationToken)).ToList();

                // Ensure there are no unexpected file extensions within the symbol package.
                if (!AreSymbolFilesValid(files))
                {
                    return null;
                }

                return files.Where(p => Path.GetExtension(p) == ".pdb").ToList();
            }
            catch (Exception)
            {
                // TODO: ValidatePackageEntries throws PackagingException.
                // Add better logging.
                return null;
            }
        }

        private bool AreSymbolFilesValid(IReadOnlyList<string> entries)
        {
            // TODO: Validate that all PDBs are portable. See: https://github.com/NuGet/NuGetGallery/blob/master/src/NuGetGallery/Services/SymbolPackageService.cs#L174
            bool IsValidSymbolFileInfo(FileInfo file)
            {
                if (string.IsNullOrEmpty(file.Name)) return false;
                if (string.IsNullOrEmpty(file.Extension)) return false;
                if (!ValidSymbolPackageContentExtensions.Contains(file.Extension)) return false;

                return true;
            }

            return entries.Select(e => new FileInfo(e)).All(IsValidSymbolFileInfo);
        }

        private async Task<PortablePdb?> ExtractPortablePdbAsync(
            PackageArchiveReader symbolPackage,
            string pdbPath,
            CancellationToken cancellationToken)
        {
            // TODO: Validate that the PDB has a corresponding DLL
            // See: https://github.com/NuGet/NuGet.Jobs/blob/master/src/Validation.Symbols/SymbolsValidatorService.cs#L170
            PortablePdb result;
            using var pdbFile = fileCachePool.Register();
            
            try
            {
                await using var rawPdbStream = await symbolPackage.GetStreamAsync(pdbPath, cancellationToken);
                await using var pdbFileStream =
                    new BufferedStream(new FileStream(pdbFile.FullPath, FileMode.Create, FileAccess.Write));
                await rawPdbStream.CopyToAsync(pdbFileStream, cancellationToken);
                pdbFileStream.Close();
                rawPdbStream.Close();

                await using var pdbStream = new FileStream(pdbFile.FullPath, FileMode.Open, FileAccess.Read);
                string signature;
                using (var pdbReaderProvider = MetadataReaderProvider.FromPortablePdbStream(pdbStream, MetadataStreamOptions.LeaveOpen))
                {
                    var reader = pdbReaderProvider.GetMetadataReader();
                    var id = new BlobContentId(reader.DebugMetadataHeader!.Id);

                    signature = id.Guid.ToString("N").ToUpperInvariant();
                }

                var fileName = Path.GetFileName(pdbPath).ToLowerInvariant();
                var key = $"{signature}ffffffff";

                pdbStream.Position = 0;
                result = new PortablePdb(fileName, key, pdbStream);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to extract portable PDB {PdbPath}", pdbPath);
                return null;
            }

            return result;
        }

        private class PortablePdb : IDisposable
        {
            public PortablePdb(string filename, string key, Stream content)
            {
                Filename = filename;
                Key = key;
                Content = content;
            }

            public string Filename { get; }
            public string Key { get; }
            public Stream Content { get; }

            public void Dispose() => Content.Dispose();
        }

        private class PdbList : List<PortablePdb>, IDisposable
        {
            public void Dispose()
            {
                foreach (var pdb in this)
                {
                    pdb.Dispose();
                }
            }
        }
    }
}
