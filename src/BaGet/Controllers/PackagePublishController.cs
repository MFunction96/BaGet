using BaGet.Core;
using BaGet.Core.Configuration;
using BaGet.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Versioning;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace BaGet.Controllers
{
    [ApiController]
    [Route("api/v2")]
    public class PackagePublishController(
        IAuthenticationService authentication,
        IPackageIndexingService indexer,
        IPackageDatabase packages,
        IPackageDeletionService deletionService,
        IOptions<BaGetOptions> options,
        IFileCachePool fileCachePool,
        ILogger<PackagePublishController> logger)
        : ControllerBase
    {
        // See: https://docs.microsoft.com/en-us/nuget/api/package-publish-resource#push-a-package
        [HttpPut("package", Name = Routes.UploadPackageRouteName)]
        public async Task Upload(CancellationToken cancellationToken)
        {
            if (options.Value.IsReadOnlyMode ||
                !await authentication.AuthenticateAsync(Request.GetApiKey(), cancellationToken))
            {
                HttpContext.Response.StatusCode = 401;
                return;
            }

            try
            {
                using var uploadFile = fileCachePool.Register();
                await using var uploadStream = await Request.GetUploadStreamOrNullAsync(uploadFile, cancellationToken);
                var result = await indexer.IndexAsync(uploadStream, cancellationToken);

                HttpContext.Response.StatusCode = result switch
                {
                    PackageIndexingResult.InvalidPackage => 400,
                    PackageIndexingResult.PackageAlreadyExists => 409,
                    PackageIndexingResult.Success => 201,
                    _ => HttpContext.Response.StatusCode
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception thrown during package upload");

                HttpContext.Response.StatusCode = 500;
            }
        }

        [HttpDelete("package/{id}/{version}", Name = Routes.DeleteRouteName)]
        public async Task<IActionResult> Delete(string id, string version, CancellationToken cancellationToken)
        {
            if (options.Value.IsReadOnlyMode)
            {
                return Unauthorized();
            }

            if (!NuGetVersion.TryParse(version, out var nugetVersion))
            {
                return NotFound();
            }

            if (!await authentication.AuthenticateAsync(Request.GetApiKey(), cancellationToken))
            {
                return Unauthorized();
            }

            if (await deletionService.TryDeletePackageAsync(id, nugetVersion, cancellationToken))
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("package/{id}/{version}", Name = Routes.RelistRouteName)]
        public async Task<IActionResult> Relist(string id, string version, CancellationToken cancellationToken)
        {
            if (options.Value.IsReadOnlyMode)
            {
                return Unauthorized();
            }

            if (!NuGetVersion.TryParse(version, out var nugetVersion))
            {
                return NotFound();
            }

            if (!await authentication.AuthenticateAsync(Request.GetApiKey(), cancellationToken))
            {
                return Unauthorized();
            }

            if (await packages.RelistPackageAsync(id, nugetVersion, cancellationToken))
            {
                return Ok();
            }

            return NotFound();
        }
    }
}
