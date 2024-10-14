using System;
using System.Collections.Generic;
using System.Linq;
using BaGet.Protocol.Models;

namespace BaGet.Core
{
    public class RegistrationBuilder
    {
        private readonly IUrlGenerator _url;

        public RegistrationBuilder(IUrlGenerator url)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public virtual BaGetRegistrationIndexResponse BuildIndex(PackageRegistration registration)
        {
            var sortedPackages = registration.Packages.OrderBy(p => p.Version).ToList();

            // TODO: Paging of registration items.
            // "Un-paged" example: https://api.nuget.org/v3/registration3/newtonsoft.json/index.json
            // Paged example: https://api.nuget.org/v3/registration3/fake/index.json
            return new BaGetRegistrationIndexResponse
            {
                RegistrationIndexUrl = _url.GetRegistrationIndexUrl(registration.PackageId),
                Type = RegistrationIndexResponse.DefaultType,
                Count = 1,
                TotalDownloads = registration.Packages.Sum(p => p.Downloads),
                Pages = new[]
                {
                    new BaGetRegistrationIndexPage
                    {
                        RegistrationPageUrl = _url.GetRegistrationIndexUrl(registration.PackageId),
                        Count = registration.Packages.Count(),
                        Lower = sortedPackages.First().Version.ToNormalizedString().ToLowerInvariant(),
                        Upper = sortedPackages.Last().Version.ToNormalizedString().ToLowerInvariant(),
                        ItemsOrNull = sortedPackages.Select(ToRegistrationIndexPageItem).ToList(),
                    }
                }
            };
        }

        public virtual RegistrationLeafResponse BuildLeaf(Package package)
        {
            var id = package.Id;
            var version = package.Version;

            return new RegistrationLeafResponse
            {
                Type = RegistrationLeafResponse.DefaultType,
                Listed = package.Listed,
                Published = package.Published,
                RegistrationLeafUrl = _url.GetRegistrationLeafUrl(id, version),
                PackageContentUrl = _url.GetPackageDownloadUrl(id, version),
                RegistrationIndexUrl = _url.GetRegistrationIndexUrl(id)
            };
        }

        private BaGetRegistrationIndexPageItem ToRegistrationIndexPageItem(Package package) =>
            new BaGetRegistrationIndexPageItem
            {
                RegistrationLeafUrl = _url.GetRegistrationLeafUrl(package.Id, package.Version),
                PackageContentUrl = _url.GetPackageDownloadUrl(package.Id, package.Version),
                PackageMetadata = new BaGetPackageMetadata
                {
                    PackageId = package.Id,
                    Version = package.Version.ToFullString(),
                    Authors = string.Join(", ", package.Authors),
                    Description = package.Description,
                    Downloads = package.Downloads,
                    HasReadme = package.HasReadme,
                    IconUrl = package.HasEmbeddedIcon
                        ? _url.GetPackageIconDownloadUrl(package.Id, package.Version)
                        : (package.IconUrl ?? string.Empty),
                    Language = package.Language,
                    LicenseUrl = (package.LicenseUrl ?? string.Empty),
                    Listed = package.Listed,
                    MinClientVersion = package.MinClientVersion,
                    ReleaseNotes = package.ReleaseNotes ?? string.Empty,
                    PackageContentUrl = _url.GetPackageDownloadUrl(package.Id, package.Version),
                    PackageTypes = package.PackageTypes.Where(t => !string.IsNullOrEmpty(t.Name)).Select(t => t.Name!).ToList(),
                    ProjectUrl = (package.ProjectUrl ?? string.Empty),
                    RepositoryUrl = package.RepositoryUrl ?? string.Empty,
                    RepositoryType = package.RepositoryType,
                    Published = package.Published == DateTime.MinValue ? DateTimeOffset.MinValue : package.Published,
                    RequireLicenseAcceptance = package.RequireLicenseAcceptance,
                    Summary = package.Summary,
                    Tags = package.Tags?.ToList() ?? new List<string>(),
                    Title = package.Title,
                    DependencyGroups = ToDependencyGroups(package)
                },
            };

        private IReadOnlyList<DependencyGroupItem> ToDependencyGroups(Package package)
        {
            return package.Dependencies
                .GroupBy(d => d.TargetFramework)
                .Select(group => new DependencyGroupItem
                {
                    TargetFramework = group.Key,

                    // A package that supports a target framework but does not have dependencies while on
                    // that target framework is represented by a fake dependency with a null "Id" and "VersionRange".
                    // This fake dependency should not be included in the output.
                    Dependencies = group
                        .Where(d => d.Id != null && d.VersionRange != null)
                        .Select(d => new DependencyItem
                        {
                            Id = d.Id,
                            Range = d.VersionRange
                        })
                        .ToList()
                })
                .ToList();
        }
    }
}
