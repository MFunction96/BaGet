using BaGet.Core;
using BaGet.Core.Configuration;
using BaGet.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace BaGet.Controllers
{
    [ApiController]
    [Route("api")]
    public class SymbolController(
        IAuthenticationService authentication,
        ISymbolIndexingService indexer,
        ISymbolStorageService storage,
        IOptions<BaGetOptions> options,
        IFileCachePool fileCachePool,
        ILogger<SymbolController> logger)
        : ControllerBase
    {
        // See: https://docs.microsoft.com/en-us/nuget/api/package-publish-resource#push-a-package
        [HttpPut("v2/symbol", Name = Routes.UploadSymbolRouteName)]
        public async Task Upload(CancellationToken cancellationToken)
        {
            if (options.Value.IsReadOnlyMode || !await authentication.AuthenticateAsync(Request.GetApiKey(), cancellationToken))
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
                    SymbolIndexingResult.InvalidSymbolPackage => 400,
                    SymbolIndexingResult.PackageNotFound => 404,
                    SymbolIndexingResult.Success => 201,
                    _ => HttpContext.Response.StatusCode
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception thrown during symbol upload");

                HttpContext.Response.StatusCode = 500;
            }
        }

        [HttpGet("download/symbols/{file}/{key}", Name = Routes.SymbolDownloadRouteName)]
        public async Task<IActionResult> Get(string file, string key)
        {
            var pdbStream = await storage.GetPortablePdbContentStreamOrNullAsync(file, key);

            return File(pdbStream, "application/octet-stream");
        }

        [HttpGet("download/symbols/{prefix}/{file}/{key}/{file2}", Name = Routes.PrefixedSymbolDownloadRouteName)]
        public Task<IActionResult> Get(string prefix, string file, string key, string file2)
        {
            throw new NotImplementedException();
        }
    }
}
