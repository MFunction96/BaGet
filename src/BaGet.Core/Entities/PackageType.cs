using System.ComponentModel.DataAnnotations;
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
        [MaxLength(AbstractContext<DbContext>.MaxPackageTypeNameLength)]
        public string? Name { get; set; }
        [Unicode]
        [MaxLength(AbstractContext<DbContext>.MaxPackageTypeVersionLength)]
        public string? Version { get; set; }

        public Package Package { get; set; } = null!;
    }
}
