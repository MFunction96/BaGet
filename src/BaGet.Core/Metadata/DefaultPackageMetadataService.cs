using BaGet.Protocol.Models;
using NuGet.Versioning;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Core.Metadata
{
    /// <inheritdoc />
    public class DefaultPackageMetadataService(
        IPackageService packages,
        RegistrationBuilder builder)
        : IPackageMetadataService
    {
        public async Task<BaGetRegistrationIndexResponse?> GetRegistrationIndexOrNullAsync(
            string packageId,
            CancellationToken cancellationToken = default)
        {
            var packages1 = await packages.FindPackagesAsync(packageId, cancellationToken);
            if (!packages1.Any())
            {
                return null;
            }

            return builder.BuildIndex(
                new PackageRegistration(
                    packageId,
                    packages1));
        }

        public async Task<RegistrationLeafResponse?> GetRegistrationLeafOrNullAsync(
            string id,
            NuGetVersion version,
            CancellationToken cancellationToken = default)
        {
            var package = await packages.FindPackageOrNullAsync(id, version, cancellationToken);
            if (package is null)
            {
                return null;
            }

            return builder.BuildLeaf(package);
        }
    }
}
