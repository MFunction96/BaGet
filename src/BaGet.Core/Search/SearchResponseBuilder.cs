using BaGet.Core.Metadata;
using BaGet.Protocol.Models;
using System.Collections.Generic;
using System.Linq;

namespace BaGet.Core.Search
{
    public class SearchResponseBuilder(IUrlGenerator url) : ISearchResponseBuilder
    {
        public SearchResponse BuildSearch(IEnumerable<PackageRegistration> packageRegistrations)
        {
            var result = new List<SearchResult>();

            foreach (var packageRegistration in packageRegistrations)
            {
                var versions = packageRegistration.Packages.OrderByDescending(p => p.Version).ToList();
                var latest = versions.First();
                var iconUrl = latest.HasEmbeddedIcon
                    ? url.GetPackageIconDownloadUrl(latest.Id, latest.Version)
                    : latest.IconUrl?.AbsoluteUri;

                result.Add(new SearchResult
                {
                    PackageId = latest.Id,
                    Version = latest.Version.ToFullString(),
                    Description = latest.Description,
                    Authors = latest.Authors.ToList(),
                    IconUrl = iconUrl ?? string.Empty,
                    LicenseUrl = latest.LicenseUrl?.AbsoluteUri ?? string.Empty,
                    ProjectUrl = latest.ProjectUrl?.AbsoluteUri ?? string.Empty,
                    RegistrationIndexUrl = url.GetRegistrationIndexUrl(latest.Id),
                    Summary = latest.Summary,
                    Tags = latest.Tags?.ToList() ?? [],
                    Title = latest.Title,
                    TotalDownloads = versions.Sum(p => p.Downloads),
                    Versions = versions
                        .Select(p => new SearchResultVersion
                        {
                            RegistrationLeafUrl = url.GetRegistrationLeafUrl(p.Id, p.Version),
                            Version = p.Version.ToFullString(),
                            Downloads = p.Downloads,
                        })
                        .ToList(),
                });
            }

            return new SearchResponse
            {
                TotalHits = result.Count,
                Data = result,
                Context = SearchContext.Default(url.GetPackageMetadataResourceUrl()),
            };
        }

        public AutocompleteResponse BuildAutocomplete(IReadOnlyList<string> data)
        {
            return new AutocompleteResponse
            {
                TotalHits = data.Count,
                Data = data,
                Context = AutocompleteContext.Default
            };
        }

        public DependentsResponse BuildDependents(IReadOnlyList<PackageDependent> packages)
        {
            return new DependentsResponse
            {
                TotalHits = packages.Count,
                Data = packages,
            };
        }
    }
}
