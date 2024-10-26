using BaGet.Core.Entities;
using BaGet.Core.Extensions;
using BaGet.Core.Indexing;
using BaGet.Core.Metadata;
using BaGet.Protocol.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Core.Search
{
    public class DatabaseSearchService(
        BaGetDbContext context,
        IFrameworkCompatibilityService frameworksCompatibilityService,
        ISearchResponseBuilder searchBuilder,
        ILogger<DatabaseSearchService> logger)
        : ISearchService
    {
        public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            var frameworks = GetCompatibleFrameworksOrNull(request.Framework);

            IQueryable<Package> search = context.Packages;
            if (!string.IsNullOrEmpty(request.Query))
            {
                search = search.ApplySearchQuery(request.Query);
            }

            search = search.ApplySearchFilters(request.IncludePrerelease, request.IncludeSemVer2, request.PackageType, frameworks);

            var results = await context.Packages.ToArrayAsync(cancellationToken);

            var groupedResults = results
                .GroupBy(p => p.Id)
                .Select(group => new PackageRegistration(group.Key, group.ToArray()))
                .AsEnumerable()
                .ToList();

            return searchBuilder.BuildSearch(groupedResults);
        }

        public async Task<AutocompleteResponse> AutocompleteAsync(
            AutocompleteRequest request,
            CancellationToken cancellationToken)
        {
            IQueryable<Package> search = context.Packages;

            search = search.ApplySearchQuery(request.Query);
            search = search.ApplySearchFilters(
                request.IncludePrerelease,
                request.IncludeSemVer2,
                request.PackageType,
                frameworks: null);

            var packageIds = await search
                .OrderByDescending(p => p.Downloads)
                .Select(p => p.Id)
                .Distinct()
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync(cancellationToken);

            return searchBuilder.BuildAutocomplete(packageIds);
        }

        public async Task<AutocompleteResponse> ListPackageVersionsAsync(
            VersionsRequest request,
            CancellationToken cancellationToken)
        {
            var packageId = request.PackageId.ToLower();
            var search = context
                .Packages
                .Where(p => p.Id.ToLower().Equals(packageId));

            search = search.ApplySearchFilters(
                request.IncludePrerelease,
                request.IncludeSemVer2,
                packageType: null,
                frameworks: null);

            var packageVersions = await search
                .Select(p => p.NormalizedVersionString)
                .ToListAsync(cancellationToken);

            return searchBuilder.BuildAutocomplete(packageVersions);
        }

        public async Task<DependentsResponse> FindDependentsAsync(string packageId, CancellationToken cancellationToken)
        {
            var dependents = await context
                .Packages
                .Where(p => p.Listed)
                .OrderByDescending(p => p.Downloads)
                .Where(p => p.Dependencies.Any(d => d.Id == packageId))
                .Take(20)
                .Select(r => new PackageDependent
                {
                    Id = r.Id,
                    Description = r.Description,
                    TotalDownloads = r.Downloads
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            return searchBuilder.BuildDependents(dependents);
        }

        private IEnumerable<string>? GetCompatibleFrameworksOrNull(string? framework)
        {
            return string.IsNullOrEmpty(framework) ? null : frameworksCompatibilityService.FindAllCompatibleFrameworks(framework);
        }
    }
}
