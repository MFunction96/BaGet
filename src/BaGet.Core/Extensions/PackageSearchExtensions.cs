using BaGet.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaGet.Core.Extensions
{
    public static class PackageSearchExtensions
    {
        public static IQueryable<Package> ApplySearchQuery(this IQueryable<Package> query, string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return query;
            }

            search = search.ToLowerInvariant();

            return query.Where(p => p.Id.ToLower().Contains(search));
        }

        public static IQueryable<Package> ApplySearchFilters(
            this IQueryable<Package> query,
            bool includePrerelease,
            bool includeSemVer2,
            string? packageType,
            IEnumerable<string>? frameworks)
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
    }
}
