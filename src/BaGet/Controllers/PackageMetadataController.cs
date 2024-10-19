using BaGet.Core;
using BaGet.Core.Metadata;
using BaGet.Protocol.Models;
using Microsoft.AspNetCore.Mvc;
using NuGet.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Controllers
{
    /// <summary>
    /// The Package Metadata resource, used to fetch packages' information.
    /// See: https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource
    /// </summary>
    [ApiController]
    [Route("v3/registration")]
    public class PackageMetadataController(IPackageMetadataService metadata) : ControllerBase
    {
        // GET v3/registration/{id}.json
        [HttpGet("{id}/index.json")]
        public async Task<ActionResult<BaGetRegistrationIndexResponse>> RegistrationIndexAsync(string id, CancellationToken cancellationToken)
        {
            var index = await metadata.GetRegistrationIndexOrNullAsync(id, cancellationToken);
            if (index is null)
            {
                return NotFound();
            }

            return index;
        }

        // GET v3/registration/{id}/{version}.json
        [HttpGet("{id}/{version}.json")]
        public async Task<ActionResult<RegistrationLeafResponse>> RegistrationLeafAsync(string id, string version, CancellationToken cancellationToken)
        {
            if (!NuGetVersion.TryParse(version, out var nugetVersion))
            {
                return NotFound();
            }

            var leaf = await metadata.GetRegistrationLeafOrNullAsync(id, nugetVersion, cancellationToken);
            if (leaf is null)
            {
                return NotFound();
            }

            return leaf;
        }
    }
}
