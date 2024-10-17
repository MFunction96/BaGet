using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using BaGet.Core.Entities;

namespace BaGet.Core
{
    // See NuGetGallery.Core's: https://github.com/NuGet/NuGetGallery/blob/master/src/NuGetGallery.Core/Entities/PackageDependency.cs
    [Index(nameof(Id))]
    public class PackageDependency
    {
        [Key]
        public int Key { get; set; }

        /// <summary>
        /// The dependency's package ID. Null if this is a dependency group without any dependencies.
        /// </summary>
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageIdLength)]
        public string? Id { get; set; }

        /// <summary>
        /// The dependency's package version. Null if this is a dependency group without any dependencies.
        /// </summary>
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageDependencyVersionRangeLength)]
        public string? VersionRange { get; set; }

        [Unicode]
        [MaxLength(BaGetDbContext.MaxTargetFrameworkLength)]
        public string? TargetFramework { get; set; }

        public Package? Package { get; set; }
    }
}
