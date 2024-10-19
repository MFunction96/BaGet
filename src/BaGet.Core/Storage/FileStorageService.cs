using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File;

namespace BaGet.Core.Storage
{
    /// <summary>
    /// Stores content on disk.
    /// </summary>
    public class FileStorageService : IStorageService
    {
        // See: https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/IO/Stream.cs#L35
        private const int DefaultCopyBufferSize = 81920;

        private readonly string _storePath;

        public FileStorageService(IOptions<FileSystemStorageOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // Resolve relative path components ('.'/'..') and ensure there is a trailing slash.
            _storePath = Path.GetFullPath(options.Value.Path);
            if (!_storePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                _storePath += Path.DirectorySeparatorChar;
        }

        public Task<Stream> GetAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            path = GetFullPath(path);
            var content = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            return Task.FromResult<Stream>(content);
        }

        public Task<Uri> GetDownloadUriAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new Uri(GetFullPath(path));

            return Task.FromResult(result);
        }

        public async Task<StoragePutResult> PutAsync(
            string path,
            Stream content,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(contentType)) throw new ArgumentException("Content type is required", nameof(contentType));

            cancellationToken.ThrowIfCancellationRequested();

            path = GetFullPath(path);

            // Ensure that the path exists.
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            try
            {
                await using var fileStream = new BufferedStream(new FileStream(path, FileMode.CreateNew, FileAccess.Write));
                await content.CopyToAsync(fileStream, DefaultCopyBufferSize, cancellationToken);
                return StoragePutResult.Success;
            }
            catch (IOException) when (File.Exists(path))
            {
                var checksum = new Checksum(SHAAlgorithm.SHA256);
                var contentChecksum = (await checksum.GetStreamHashAsync(content, cancellationToken))!;
                var pathChecksum = (await checksum.GetFileHashAsync(path, cancellationToken))!;
                return contentChecksum.SequenceEqual(pathChecksum)
                    ? StoragePutResult.AlreadyExists
                    : StoragePutResult.Conflict;
            }
        }

        public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                File.Delete(GetFullPath(path));
            }
            catch (DirectoryNotFoundException)
            {
            }

            return Task.CompletedTask;
        }

        private string GetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path is required", nameof(path));
            }

            var fullPath = Path.GetFullPath(Path.Combine(_storePath, path));

            // Verify path is under the _storePath.
            if (!fullPath.StartsWith(_storePath, StringComparison.Ordinal) ||
                fullPath.Length == _storePath.Length)
            {
                throw new ArgumentException("Path resolves outside store path", nameof(path));
            }

            return fullPath;
        }
    }
}
