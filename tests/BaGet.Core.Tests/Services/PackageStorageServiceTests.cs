using BaGet.Core.Entities;
using BaGet.Core.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File.Cache;
using Xunit;

namespace BaGet.Core.Tests.Services
{
    public class PackageStorageServiceTests
    {
        public class SavePackageContentAsync : FactsBase
        {
            [Fact]
            public async Task SavesContent()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Success);
                using var nuspecCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(nuspecCacheFile.FullPath, "My nuspec");
                using var readmeCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(readmeCacheFile.FullPath, "My readme");
                using var iconCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(iconCacheFile.FullPath, "My icon");
                await using var packageStream = StringStream("My package");
                // Act
                await _target.SavePackageContentAsync(
                    _package,
                    packageStream: packageStream,
                    nuspecCacheFile: nuspecCacheFile,
                    readmeCacheFile: readmeCacheFile,
                    iconCacheFile: iconCacheFile);

                // Assert
                Assert.True(_puts.ContainsKey(PackagePath));
                Assert.Equal("My package", _puts[PackagePath].Content);
                Assert.Equal("binary/octet-stream", _puts[PackagePath].ContentType);

                Assert.True(_puts.ContainsKey(NuspecPath));
                Assert.Equal("My nuspec", _puts[NuspecPath].Content);
                Assert.Equal("text/plain", _puts[NuspecPath].ContentType);

                Assert.True(_puts.ContainsKey(ReadmePath));
                Assert.Equal("My readme", _puts[ReadmePath].Content);
                Assert.Equal("text/markdown", _puts[ReadmePath].ContentType);

                Assert.True(_puts.ContainsKey(IconPath));
                Assert.Equal("My icon", _puts[IconPath].Content);
                Assert.Equal("image/xyz", _puts[IconPath].ContentType);
            }

            [Fact]
            public async Task DoesNotSaveReadmeIfItIsNull()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Success);
                using var nuspecCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(nuspecCacheFile.FullPath, "My nuspec");
                using (var packageStream = StringStream("My package"))
                {
                    // Act
                    await _target.SavePackageContentAsync(
                        _package,
                        packageStream: packageStream,
                        nuspecCacheFile: nuspecCacheFile,
                        readmeCacheFile: null,
                        iconCacheFile: null);
                }

                // Assert
                Assert.False(_puts.ContainsKey(ReadmePath));
            }

            [Fact]
            public async Task NormalizesVersionWhenContentIsSaved()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Success);
                using var nuspecCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(nuspecCacheFile.FullPath, "My nuspec");
                using var readmeCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(readmeCacheFile.FullPath, "My readme");
                using var iconCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(iconCacheFile.FullPath, "My icon");
                _package.Version = new NuGetVersion("1.2.3.0");
                using (var packageStream = StringStream("My package"))
                {
                    // Act
                    await _target.SavePackageContentAsync(
                        _package,
                        packageStream: packageStream,
                        nuspecCacheFile: nuspecCacheFile,
                        readmeCacheFile: readmeCacheFile,
                        iconCacheFile: iconCacheFile);
                }

                // Assert
                Assert.True(_puts.ContainsKey(PackagePath));
                Assert.True(_puts.ContainsKey(NuspecPath));
                Assert.True(_puts.ContainsKey(ReadmePath));
            }

            [Fact]
            public async Task DoesNotThrowIfContentAlreadyExistsAndContentsMatch()
            {
                // Arrange
                SetupPutResult(StoragePutResult.AlreadyExists);
                using var nuspecCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(nuspecCacheFile.FullPath, "My nuspec");
                using var readmeCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(readmeCacheFile.FullPath, "My readme");
                using var iconCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(iconCacheFile.FullPath, "My icon");
                await using var packageStream = StringStream("My package");
                await _target.SavePackageContentAsync(
                    _package,
                    packageStream: packageStream,
                    nuspecCacheFile: nuspecCacheFile,
                    readmeCacheFile: readmeCacheFile,
                    iconCacheFile: iconCacheFile);

                // Assert
                Assert.True(_puts.ContainsKey(PackagePath));
                Assert.Equal("My package", _puts[PackagePath].Content);
                Assert.Equal("binary/octet-stream", _puts[PackagePath].ContentType);

                Assert.True(_puts.ContainsKey(NuspecPath));
                Assert.Equal("My nuspec", _puts[NuspecPath].Content);
                Assert.Equal("text/plain", _puts[NuspecPath].ContentType);

                Assert.True(_puts.ContainsKey(ReadmePath));
                Assert.Equal("My readme", _puts[ReadmePath].Content);
                Assert.Equal("text/markdown", _puts[ReadmePath].ContentType);

                Assert.True(_puts.ContainsKey(IconPath));
                Assert.Equal("My icon", _puts[IconPath].Content);
                Assert.Equal("image/xyz", _puts[IconPath].ContentType);
            }

            [Fact]
            public async Task ThrowsIfContentAlreadyExistsButContentsDoNotMatch()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Conflict);
                using var nuspecCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(nuspecCacheFile.FullPath, "My nuspec");
                using var readmeCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(readmeCacheFile.FullPath, "My readme");
                using var iconCacheFile = _fileCachePool.Register();
                await File.WriteAllTextAsync(iconCacheFile.FullPath, "My icon");
                using (var packageStream = StringStream("My package"))
                {
                    // Act
                    await Assert.ThrowsAsync<InvalidOperationException>(() =>
                        _target.SavePackageContentAsync(
                            _package,
                            packageStream: packageStream,
                            nuspecCacheFile: nuspecCacheFile,
                            readmeCacheFile: readmeCacheFile,
                            iconCacheFile: iconCacheFile));
                }
            }
        }

        public class GetPackageStreamAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfStorageThrows()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                _storage
                    .Setup(s => s.GetAsync(PackagePath, cancellationToken))
                    .ThrowsAsync(new DirectoryNotFoundException());

                // Act
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    _target.GetPackageStreamAsync(_package.Id, _package.Version, cancellationToken));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                using (var packageStream = StringStream("My package"))
                {
                    _storage
                        .Setup(s => s.GetAsync(PackagePath, cancellationToken))
                        .ReturnsAsync(packageStream);

                    // Act
                    var result = await _target.GetPackageStreamAsync(_package.Id, _package.Version, cancellationToken);

                    // Assert
                    Assert.Equal("My package", await ToStringAsync(result));

                    _storage.Verify(s => s.GetAsync(PackagePath, cancellationToken), Times.Once);
                }
            }
        }

        public class GetnuspecCacheFileAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfDoesntExist()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                _storage
                    .Setup(s => s.GetAsync(NuspecPath, cancellationToken))
                    .ThrowsAsync(new DirectoryNotFoundException());

                // Act
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    _target.GetNuspecStreamAsync(_package.Id, _package.Version, cancellationToken));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                using (var nuspecCacheFile = StringStream("My nuspec"))
                {
                    _storage
                        .Setup(s => s.GetAsync(NuspecPath, cancellationToken))
                        .ReturnsAsync(nuspecCacheFile);

                    // Act
                    var result = await _target.GetNuspecStreamAsync(_package.Id, _package.Version, cancellationToken);

                    // Assert
                    Assert.Equal("My nuspec", await ToStringAsync(result));

                    _storage.Verify(s => s.GetAsync(NuspecPath, cancellationToken), Times.Once);
                }
            }
        }

        public class GetreadmeCacheFileAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfDoesntExist()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                _storage
                    .Setup(s => s.GetAsync(ReadmePath, cancellationToken))
                    .ThrowsAsync(new DirectoryNotFoundException());

                // Act
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    _target.GetReadmeStreamAsync(_package.Id, _package.Version, cancellationToken));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                var cancellationToken = CancellationToken.None;
                using (var readmeCacheFile = StringStream("My readme"))
                {
                    _storage
                        .Setup(s => s.GetAsync(ReadmePath, cancellationToken))
                        .ReturnsAsync(readmeCacheFile);

                    // Act
                    var result = await _target.GetReadmeStreamAsync(_package.Id, _package.Version, cancellationToken);

                    // Assert
                    Assert.Equal("My readme", await ToStringAsync(result));

                    _storage.Verify(s => s.GetAsync(ReadmePath, cancellationToken), Times.Once);
                }
            }
        }

        public class DeleteAsync : FactsBase
        {
            [Fact]
            public async Task Deletes()
            {
                // Act
                var cancellationToken = CancellationToken.None;
                await _target.DeleteAsync(_package.Id, _package.Version, cancellationToken);

                _storage.Verify(s => s.DeleteAsync(PackagePath, cancellationToken), Times.Once);
                _storage.Verify(s => s.DeleteAsync(NuspecPath, cancellationToken), Times.Once);
                _storage.Verify(s => s.DeleteAsync(ReadmePath, cancellationToken), Times.Once);
            }
        }

        public class FactsBase
        {
            protected readonly Package _package = new Package
            {
                Id = "My.Package",
                Version = new NuGetVersion("1.2.3")
            };

            protected readonly IFileCachePool _fileCachePool;
            protected readonly Mock<IStorageService> _storage;
            protected readonly PackageStorageService _target;
            protected readonly Mock<ILogger<FileCachePool>> _logger;

            protected readonly Dictionary<string, (string Content, string ContentType)> _puts;

            public FactsBase()
            {
                _storage = new Mock<IStorageService>();
                _logger = new Mock<ILogger<FileCachePool>>();
                _target = new PackageStorageService(_storage.Object, Mock.Of<ILogger<PackageStorageService>>());
                _fileCachePool = new FileCachePool(_logger.Object);
                _puts = new Dictionary<string, (string Content, string ContentType)>();
            }

            protected string PackagePath => Path.Combine("packages", "my.package", "1.2.3", "my.package.1.2.3.nupkg");
            protected string NuspecPath => Path.Combine("packages", "my.package", "1.2.3", "my.package.nuspec");
            protected string ReadmePath => Path.Combine("packages", "my.package", "1.2.3", "readme");
            protected string IconPath => Path.Combine("packages", "my.package", "1.2.3", "icon");

            protected Stream StringStream(string input)
            {
                var bytes = Encoding.UTF8.GetBytes(input);

                return new MemoryStream(bytes);
            }

            protected async Task<string> ToStringAsync(Stream input)
            {
                using (var reader = new StreamReader(input))
                {
                    return await reader.ReadToEndAsync();
                }
            }

            protected void SetupPutResult(StoragePutResult result)
            {
                _storage
                    .Setup(
                        s => s.PutAsync(
                            It.IsAny<string>(),
                            It.IsAny<Stream>(),
                            It.IsAny<string>(),
                            It.IsAny<CancellationToken>()))
                    .Callback((string path, Stream content, string contentType, CancellationToken cancellationToken) =>
                    {
                        using var reader = new StreamReader(content);
                        _puts[path] = (reader.ReadToEnd(), contentType);
                    })
                    .ReturnsAsync(result);
            }
        }
    }
}
