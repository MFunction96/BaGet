using BaGet.Core;
using BaGet.Core.Search;
using BaGet.Protocol.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Controllers
{
    [ApiController]
    [Route("v3")]
    public class SearchController(ISearchService searchService) : ControllerBase
    {
        [HttpGet("search", Name = Routes.SearchRouteName)]
        public async Task<ActionResult<SearchResponse>> SearchAsync(
            [FromQuery(Name = "q")] string query = "",
            [FromQuery]int pageIndex = 1,
            [FromQuery]int pageCount = 20,
            [FromQuery]bool prerelease = false,
            [FromQuery]string semVerLevel = "",

            // These are unofficial parameters
            [FromQuery]string packageType = "",
            [FromQuery]string framework = "",
            CancellationToken cancellationToken = default)
        {
            var request = new SearchRequest
            {
                PageIndex = pageIndex,
                PageCount = pageCount,
                IncludePrerelease = prerelease,
                IncludeSemVer2 = semVerLevel.Trim() == "2.0.0",
                PackageType = packageType.Trim(),
                Framework = framework.Trim(),
                Query = query.Trim(),
            };

            return await searchService.SearchAsync(request, cancellationToken);
        }

        [HttpGet("autocomplete", Name = Routes.AutocompleteRouteName)]
        public async Task<ActionResult<AutocompleteResponse>> AutocompleteAsync(
            [FromQuery(Name = "q")] string autocompleteQuery = "",
            [FromQuery(Name = "id")] string versionsQuery = "",
            [FromQuery]bool prerelease = false,
            [FromQuery]string semVerLevel = "",
            [FromQuery]int skip = 0,
            [FromQuery]int take = 20,

            // These are unofficial parameters
            [FromQuery]string packageType = "",
            CancellationToken cancellationToken = default)
        {
            var autocompleteQueryTrimmed = autocompleteQuery.Trim();
            var versionsQueryTrimmed = versionsQuery.Trim();
            var semVerLevelTrimmed = semVerLevel.Trim();
            // If only "id" is provided, find package versions. Otherwise, find package IDs.
            if (!string.IsNullOrEmpty(versionsQuery) && string.IsNullOrEmpty(autocompleteQueryTrimmed))
            {
                var request = new VersionsRequest
                {
                    IncludePrerelease = prerelease,
                    IncludeSemVer2 = semVerLevelTrimmed == "2.0.0",
                    PackageId = versionsQueryTrimmed,
                };

                return await searchService.ListPackageVersionsAsync(request, cancellationToken);
            }
            else
            {
                var request = new AutocompleteRequest
                {
                    IncludePrerelease = prerelease,
                    IncludeSemVer2 = semVerLevelTrimmed == "2.0.0",
                    PackageType = packageType,
                    Skip = skip,
                    Take = take,
                    Query = autocompleteQueryTrimmed,
                };

                return await searchService.AutocompleteAsync(request, cancellationToken);
            }
        }

        [HttpGet("dependents", Name = Routes.DependentsRouteName)]
        public async Task<ActionResult<DependentsResponse>> DependentsAsync(
            [FromQuery] string packageId = "",
            CancellationToken cancellationToken = default)
        {
            var trimmedPackageId = packageId.Trim();
            if (string.IsNullOrEmpty(trimmedPackageId.Trim()))
            {
                return BadRequest();
            }

            return await searchService.FindDependentsAsync(trimmedPackageId, cancellationToken);
        }
    }
}
