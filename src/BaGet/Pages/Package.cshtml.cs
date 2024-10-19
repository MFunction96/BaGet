using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core;
using BaGet.Core.Content;
using BaGet.Core.Entities;
using BaGet.Core.Search;
using Markdig;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace BaGet.Pages
{
    public class PackageModel(
        IPackageService packages,
        IPackageContentService content,
        ISearchService search,
        IUrlGenerator url)
        : PageModel
    {
        private static readonly MarkdownPipeline MarkdownPipeline;

        static PackageModel()
        {
            MarkdownPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
        }

        public bool Found { get; private set; }

        public Package Package { get; private set; }

        public bool IsDotnetTemplate { get; private set; }
        public bool IsDotnetTool { get; private set; }
        public DateTime LastUpdated { get; private set; }
        public long TotalDownloads { get; private set; }

        public IReadOnlyList<PackageDependent> UsedBy { get; set; } = new List<PackageDependent>();
        public IReadOnlyList<DependencyGroupModel> DependencyGroups { get; private set; } = new List<DependencyGroupModel>();
        public IReadOnlyList<VersionModel> Versions { get; private set; } = new List<VersionModel>();

        public HtmlString Readme { get; private set; } = HtmlString.Empty;

        public string IconUrl { get; private set; } = string.Empty;
        public string LicenseUrl { get; private set; } = string.Empty;
        public string PackageDownloadUrl { get; private set; } = string.Empty;

        public async Task OnGetAsync(string id, string version, CancellationToken cancellationToken)
        {
            var packages1 = await packages.FindPackagesAsync(id, cancellationToken);
            var listedPackages = packages1.Where(p => p.Listed).ToList();

            // Try to find the requested version.
            if (NuGetVersion.TryParse(version, out var requestedVersion))
            {
                Package = packages1.SingleOrDefault(p => p.Version == requestedVersion);
            }

            // Otherwise try to display the latest version.
            if (Package == null)
            {
                Package = listedPackages.OrderByDescending(p => p.Version).FirstOrDefault();
            }

            if (Package == null)
            {
                Package = new Package { Id = id };
                Found = false;
                return;
            }

            var packageVersion = Package.Version;

            Found = true;
            IsDotnetTemplate = Package.PackageTypes.Any(t => t.Name.Equals("Template", StringComparison.OrdinalIgnoreCase));
            IsDotnetTool = Package.PackageTypes.Any(t => t.Name.Equals("DotnetTool", StringComparison.OrdinalIgnoreCase));
            LastUpdated = packages1.Max(p => p.Published);
            TotalDownloads = packages1.Sum(p => p.Downloads);

            var dependents = await search.FindDependentsAsync(Package.Id, cancellationToken);

            UsedBy = dependents.Data;
            DependencyGroups = ToDependencyGroups(Package);
            Versions = ToVersions(listedPackages, packageVersion);

            if (Package.HasReadme)
            {
                Readme = await GetReadmeHtmlStringOrNullAsync(Package.Id, packageVersion, cancellationToken);
            }

            IconUrl = Package.HasEmbeddedIcon
                ? url.GetPackageIconDownloadUrl(Package.Id, packageVersion)
                : (Package.IconUrl?.AbsoluteUri ?? string.Empty);
            LicenseUrl = Package.LicenseUrl?.AbsoluteUri ?? string.Empty;
            PackageDownloadUrl = url.GetPackageDownloadUrl(Package.Id, packageVersion);
        }

        private IReadOnlyList<DependencyGroupModel> ToDependencyGroups(Package package)
        {
            return package
                .Dependencies
                .GroupBy(d => d.TargetFramework)
                .Select(group =>
                {
                    return new DependencyGroupModel
                    {
                        Name = PrettifyTargetFramework(group.Key),
                        Dependencies = group
                            .Where(d => d.Id != null)
                            .Select(d => new DependencyModel
                            {
                                PackageId = d.Id,
                                VersionSpec = (d.VersionRange != null)
                                    ? VersionRange.Parse(d.VersionRange).PrettyPrint()
                                    : string.Empty
                            })
                            .ToList()
                    };
                })
                .ToList();
        }

        private string PrettifyTargetFramework(string targetFramework)
        {
            if (targetFramework == null) return "All Frameworks";

            NuGetFramework framework;
            try
            {
                framework = NuGetFramework.Parse(targetFramework);
            }
            catch (Exception)
            {
                return targetFramework;
            }

            string frameworkName;
            if (framework.Framework.Equals(FrameworkConstants.FrameworkIdentifiers.NetCoreApp,
                StringComparison.OrdinalIgnoreCase))
            {
                frameworkName = (framework.Version.Major >= 5)
                    ? ".NET"
                    : ".NET Core";
            }
            else if (framework.Framework.Equals(FrameworkConstants.FrameworkIdentifiers.NetStandard,
                StringComparison.OrdinalIgnoreCase))
            {
                frameworkName = ".NET Standard";
            }
            else if (framework.Framework.Equals(FrameworkConstants.FrameworkIdentifiers.Net,
                StringComparison.OrdinalIgnoreCase))
            {
                frameworkName = ".NET Framework";
            }
            else
            {
                frameworkName = framework.Framework;
            }

            var frameworkVersion = (framework.Version.Build == 0)
                ? framework.Version.ToString(2)
                : framework.Version.ToString(3);

            return $"{frameworkName} {frameworkVersion}";
        }

        private IReadOnlyList<VersionModel> ToVersions(IReadOnlyList<Package> packages, NuGetVersion selectedVersion)
        {
            return packages
                .Select(p => new VersionModel
                {
                    Version = p.Version,
                    Downloads = p.Downloads,
                    Selected = p.Version == selectedVersion,
                    LastUpdated = p.Published,
                })
                .OrderByDescending(m => m.Version)
                .ToList();
        }

        private async Task<HtmlString> GetReadmeHtmlStringOrNullAsync(
            string packageId,
            NuGetVersion packageVersion,
            CancellationToken cancellationToken)
        {
            string readme;
            using (var readmeStream = await content.GetPackageReadmeStreamOrNullAsync(packageId, packageVersion, cancellationToken))
            {
                if (readmeStream == null) return null;

                using (var reader = new StreamReader(readmeStream))
                {
                    readme = await reader.ReadToEndAsync();
                }
            }

            var readmeHtml = Markdown.ToHtml(readme, MarkdownPipeline);
            return new HtmlString(readmeHtml);
        }

        public class DependencyGroupModel
        {
            public string Name { get; set; }
            public IReadOnlyList<DependencyModel> Dependencies { get; set; }
        }

        // TODO: Convert this to records.
        public class DependencyModel
        {
            public string PackageId { get; set; }
            public string VersionSpec { get; set; }
        }

        // TODO: Convert this to records.
        public class VersionModel
        {
            public NuGetVersion Version { get; set; }
            public long Downloads { get; set; }
            public bool Selected { get; set; }
            public DateTime LastUpdated { get; set; }
        }
    }
}
