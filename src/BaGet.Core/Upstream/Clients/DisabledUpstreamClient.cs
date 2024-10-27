using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core.Entities;
using NuGet.Versioning;

namespace BaGet.Core
{
    /// <summary>
    /// The client used when there are no upstream package sources.
    /// </summary>
    public class DisabledUpstreamClient : IUpstreamClient
    {
        private readonly IEnumerable<NuGetVersion> _emptyVersionList = new List<NuGetVersion>();
        private readonly IEnumerable<Package> _emptyPackageList = new List<Package>();

        public Task<IEnumerable<NuGetVersion>> ListPackageVersionsAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_emptyVersionList);
        }

        public Task<IEnumerable<Package>> ListPackagesAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_emptyPackageList);
        }

        public Task<Stream> DownloadPackageOrNullAsync(
            string id,
            NuGetVersion version,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(null);
        }
    }
}
