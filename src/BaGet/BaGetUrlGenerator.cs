using BaGet.Core;
using BaGet.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NuGet.Versioning;
using System;

namespace BaGet
{
    // TODO: This should validate the "Host" header against known valid values
    public class BaGetUrlGenerator(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        : IUrlGenerator
    {
        public string GetServiceIndexUrl()
        {
            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.IndexRouteName,
                values: null);
        }

        public string GetPackageContentResourceUrl()
        {
            return AbsoluteUrl("v3/package");
        }

        public string GetPackageMetadataResourceUrl()
        {
            return AbsoluteUrl("v3/registration");
        }

        public string GetPackagePublishResourceUrl()
        {
            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.UploadPackageRouteName,
                values: null);
        }

        public string GetSymbolPublishResourceUrl()
        {
            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.UploadSymbolRouteName,
                values: null);
        }

        public string GetSearchResourceUrl()
        {
            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.SearchRouteName,
                values: null);
        }

        public string GetAutocompleteResourceUrl()
        {
            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.AutocompleteRouteName,
                values: null);
        }

        public string GetRegistrationIndexUrl(string id)
        {
            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.RegistrationIndexRouteName,
                values: new { Id = id.ToLowerInvariant() });
        }

        public string GetRegistrationPageUrl(string id, NuGetVersion lower, NuGetVersion upper)
        {
            // BaGet does not support paging the registration resource.
            throw new NotImplementedException();
        }

        public string GetRegistrationLeafUrl(string id, NuGetVersion version)
        {
            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.RegistrationLeafRouteName,
                values: new
                {
                    Id = id.ToLowerInvariant(),
                    Version = version.ToNormalizedString().ToLowerInvariant(),
                });
        }

        public string GetPackageVersionsUrl(string id)
        {
            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.PackageVersionsRouteName,
                values: new { Id = id.ToLowerInvariant() });
        }

        public string GetPackageDownloadUrl(string id, NuGetVersion version)
        {
            id = id.ToLowerInvariant();
            var versionString = version.ToNormalizedString().ToLowerInvariant();

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.PackageDownloadRouteName,
                values: new
                {
                    Id = id,
                    Version = versionString,
                    IdVersion = $"{id}.{versionString}"
                });
        }

        public string GetPackageManifestDownloadUrl(string id, NuGetVersion version)
        {
            id = id.ToLowerInvariant();
            var versionString = version.ToNormalizedString().ToLowerInvariant();

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.PackageDownloadRouteName,
                values: new
                {
                    Id = id,
                    Version = versionString,
                    Id2 = id,
                });
        }

        public string GetPackageIconDownloadUrl(string id, NuGetVersion version)
        {
            id = id.ToLowerInvariant();
            var versionString = version.ToNormalizedString().ToLowerInvariant();

            return linkGenerator.GetUriByRouteValues(
                httpContextAccessor.HttpContext,
                Routes.PackageDownloadIconRouteName,
                values: new
                {
                    Id = id,
                    Version = versionString
                });
        }

        private string AbsoluteUrl(string relativePath)
        {
            var request = httpContextAccessor.HttpContext.Request;

            return string.Concat(
                request.Scheme,
                "://",
                request.Host.ToUriComponent(),
                request.PathBase.ToUriComponent(),
                "/",
                relativePath);
        }
    }
}
