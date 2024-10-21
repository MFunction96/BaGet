using BaGet.Core.Entities;
using BaGet.Core.Indexing;
using BaGet.Core.Metadata;
using BaGet.Protocol.Models;
using Microsoft.EntityFrameworkCore;
using System;
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
        ISearchResponseBuilder searchBuilder)
        : ISearchService
    {
        public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            var frameworks = GetCompatibleFrameworksOrNull(request.Framework);

            IQueryable<Package> search = context.Packages;
            if (!string.IsNullOrEmpty(request.Query))
            {
                search = ApplySearchQuery(search, request.Query);
            }

            search = ApplySearchFilters(
                search,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                request.PackageType,
                frameworks);

            var results = search
                .Distinct()
                .OrderBy(id => id)
                .Page(request.PageIndex, request.PageCount);

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

            search = ApplySearchQuery(search, request.Query);
            search = ApplySearchFilters(
                search,
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

            search = ApplySearchFilters(
                search,
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

        private IQueryable<Package> ApplySearchQuery(IQueryable<Package> query, string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return query;
            }

            search = search.ToLowerInvariant();

            return query.Where(p => p.Id.ToLower().Contains(search));
        }

        private static IQueryable<Package> ApplySearchFilters(
            IQueryable<Package> query,
            bool includePrerelease,
            bool includeSemVer2,
            string? packageType,
            IReadOnlyList<string>? frameworks)
        {
            if (!includePrerelease)
            {
                query = query.Where(p => !p.IsPrerelease);
            }

            if (!includeSemVer2)
            {
                query = query.Where(p => p.SemVerLevel != SemVerLevel.SemVer2);
            }

            if (!string.IsNullOrEmpty(packageType))
            {
                query = query.Where(p => p.PackageTypes.Any(t => t.Name == packageType));
            }

            if (frameworks is not null)
            {
                query = query.Where(p => p.TargetFrameworks.Any(f => frameworks.Contains(f.Moniker)));
            }

            return query.Where(p => p.Listed);
        }

        private IReadOnlyList<string>? GetCompatibleFrameworksOrNull(string? framework)
        {
            return string.IsNullOrEmpty(framework) ? null : frameworksCompatibilityService.FindAllCompatibleFrameworks(framework);
        }
    }
}
