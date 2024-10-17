using System.ComponentModel.DataAnnotations;
using BaGet.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaGet.Core
{
    // See NuGetGallery.Core's: https://github.com/NuGet/NuGetGallery/blob/master/src/NuGetGallery.Core/Entities/PackageType.cs
    [Index(nameof(Name))]
    public class PackageType
    {
        [Key]
        public int Key { get; set; }
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageTypeNameLength)]
        public string? Name { get; set; }
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageTypeVersionLength)]
        public string? Version { get; set; }

        public Package Package { get; set; } = null!;
    }
}
