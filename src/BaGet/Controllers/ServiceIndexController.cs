using BaGet.Core;
using BaGet.Protocol.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Controllers
{
    /// <summary>
    /// The NuGet Service Index. This aids NuGet client to discover this server's services.
    /// </summary>
    [ApiController]
    [Route("v3")]
    public class ServiceIndexController(IServiceIndexService serviceIndex) : ControllerBase
    {
        // GET v3/index
        [HttpGet("index.json", Name = Routes.IndexRouteName)]
        public async Task<ServiceIndexResponse> GetAsync(CancellationToken cancellationToken)
        {
            return await serviceIndex.GetAsync(cancellationToken);
        }
    }
}
