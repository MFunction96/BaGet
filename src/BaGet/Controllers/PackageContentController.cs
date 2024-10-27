using BaGet.Core.Content;
using BaGet.Protocol.Models;
using Microsoft.AspNetCore.Mvc;
using NuGet.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Controllers
{
    /// <summary>
    /// The Package Content resource, used to download content from packages.
    /// See: https://docs.microsoft.com/en-us/nuget/api/package-base-address-resource
    /// </summary>
    [ApiController]
    [Route("v3/package")]
    public class PackageContentController(IPackageContentService content) : ControllerBase
    {
        [HttpGet("{id}/index.json", Name = Routes.PackageVersionsRouteName)]
        public async Task<ActionResult<PackageVersionsResponse>> GetPackageVersionsAsync(string id, CancellationToken cancellationToken)
        {
            var versions = await content.GetPackageVersionsOrNullAsync(id, cancellationToken);
            if (versions is null)
            {
                return NotFound();
            }

            return versions;
        }

        [HttpGet("{id}/{version}/{idVersion}.nupkg", Name = Routes.PackageDownloadRouteName)]
        public async Task<IActionResult> DownloadPackageAsync(string id, string version, CancellationToken cancellationToken)
        {
            if (!NuGetVersion.TryParse(version, out var nugetVersion))
            {
                return NotFound();
            }

            var packageStream = await content.GetPackageContentStreamOrNullAsync(id, nugetVersion, cancellationToken);
            if (packageStream is null)
            {
                return NotFound();
            }

            return File(packageStream, "application/octet-stream");
        }

        [HttpGet("{id}/{version}/{id2}.nuspec", Name = Routes.PackageDownloadManifestRouteName)]
        public async Task<IActionResult> DownloadNuspecAsync(string id, string version, CancellationToken cancellationToken)
        {
            if (!NuGetVersion.TryParse(version, out var nugetVersion))
            {
                return NotFound();
            }

            var nuspecStream = await content.GetPackageManifestStreamOrNullAsync(id, nugetVersion, cancellationToken);
            if (nuspecStream == null)
            {
                return NotFound();
            }

            return File(nuspecStream, "text/xml");
        }

        [HttpGet("{id}/{version}/readme", Name = Routes.PackageDownloadReadmeRouteName)]
        public async Task<IActionResult> DownloadReadmeAsync(string id, string version, CancellationToken cancellationToken)
        {
            if (!NuGetVersion.TryParse(version, out var nugetVersion))
            {
                return NotFound();
            }

            var readmeStream = await content.GetPackageReadmeStreamOrNullAsync(id, nugetVersion, cancellationToken);
            if (readmeStream == null)
            {
                return NotFound();
            }

            return File(readmeStream, "text/markdown");
        }

        [HttpGet("{id}/{version}/icon", Name = Routes.PackageDownloadIconRouteName)]
        public async Task<IActionResult> DownloadIconAsync(string id, string version, CancellationToken cancellationToken)
        {
            if (!NuGetVersion.TryParse(version, out var nugetVersion))
            {
                return NotFound();
            }

            var iconStream = await content.GetPackageIconStreamOrNullAsync(id, nugetVersion, cancellationToken);
            if (iconStream == null)
            {
                return NotFound();
            }

            return File(iconStream, "image/xyz");
        }
    }
}
