using BaGet.Protocol.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Core
{
    public class BaGetServiceIndex(IUrlGenerator urlGenerator) : IServiceIndexService
    {
        private IEnumerable<ServiceIndexItem> BuildResource(string name, string url, params string[] versions)
        {
            foreach (var version in versions)
            {
                var type = string.IsNullOrEmpty(version) ? name : $"{name}/{version}";

                yield return new ServiceIndexItem
                {
                    ResourceUrl = url,
                    Type = type,
                };
            }
        }

        public Task<ServiceIndexResponse> GetAsync(CancellationToken cancellationToken = default)
        {
            var resources = new List<ServiceIndexItem>();

            resources.AddRange(BuildResource("PackagePublish", urlGenerator.GetPackagePublishResourceUrl(), "2.0.0"));
            resources.AddRange(BuildResource("SymbolPackagePublish", urlGenerator.GetSymbolPublishResourceUrl(), "4.9.0"));
            resources.AddRange(BuildResource("SearchQueryService", urlGenerator.GetSearchResourceUrl(), "", "3.0.0-beta", "3.0.0-rc"));
            resources.AddRange(BuildResource("RegistrationsBaseUrl", urlGenerator.GetPackageMetadataResourceUrl(), "", "3.0.0-rc", "3.0.0-beta"));
            resources.AddRange(BuildResource("PackageBaseAddress", urlGenerator.GetPackageContentResourceUrl(), "3.0.0"));
            resources.AddRange(BuildResource("SearchAutocompleteService", urlGenerator.GetAutocompleteResourceUrl(), "", "3.0.0-rc", "3.0.0-beta"));

            var result = new ServiceIndexResponse
            {
                Version = "3.0.0",
                Resources = resources,
            };

            return Task.FromResult(result);
        }
    }
}
