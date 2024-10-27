using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BaGet.Core.Entities
{
    // See NuGet.Services.Entities' : https://github.com/NuGet/NuGetGallery/blob/main/src/NuGet.Services.Entities/PackageType.cs
    [Index(nameof(Name))]
    public class PackageType : IEntity
    {
        [Key]
        public int Key { get; set; }
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageTypeNameLength)]
        public string? Name { get; set; }
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageTypeVersionLength)]
        public string? Version { get; set; }

        public int PackageKey { get; set; }

        public Package Package { get; set; } = null!;
    }
}
