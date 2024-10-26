using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace BaGet.Core.Entities
{
    // See NuGetGallery's: https://github.com/NuGet/NuGetGallery/blob/main/src/NuGet.Services.Entities/Package.cs
    [Index(nameof(Id))]
    public class Package : IEntity
    {
        public ICollection<PackageDependency> Dependencies { get; set; } = new HashSet<PackageDependency>();
        public ICollection<PackageType> PackageTypes { get; set; } = new HashSet<PackageType>();
        public ICollection<TargetFramework> TargetFrameworks { get; set; } = new HashSet<TargetFramework>();

        [Key]
        public int Key { get; set; }

        [Required]
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageIdLength)]
        public string Id { get; set; } = string.Empty;

        [NotMapped]
        public NuGetVersion Version
        {
            get =>
                // Favor the original version string as it contains more information.
                // Packages uploaded with older versions of BaGet may not have the original version string.
                NuGetVersion.Parse(
                    OriginalVersionString ?? NormalizedVersionString);

            set
            {
                NormalizedVersionString = value.ToNormalizedString().ToLowerInvariant();
                OriginalVersionString = value.OriginalVersion;
            }
        }

        [Unicode]
        [Column(TypeName = "text")]
        public string Description { get; set; } = string.Empty;
        public long Downloads { get; set; }
        public bool HasReadme { get; set; }
        public bool HasEmbeddedIcon { get; set; }
        public bool IsPrerelease { get; set; }
        [Unicode]
        public List<string> Authors { get; set; } = [];
        [Unicode]
        [Column(TypeName = "text")]
        public string? ReleaseNotes { get; set; }
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageLanguageLength)]
        public string Language { get; set; } = string.Empty;
        public bool Listed { get; set; }
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageMinClientVersionLength)]
        public string MinClientVersion { get; set; } = string.Empty;
        public DateTime Published { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public SemVerLevel SemVerLevel { get; set; }
        [Unicode]
        [Column(TypeName = "text")]
        public string Summary { get; set; } = string.Empty;
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageTitleLength)]
        public string Title { get; set; } = string.Empty;
        public Uri? IconUrl { get; set; }
        public Uri? LicenseUrl { get; set; }
        public Uri? ProjectUrl { get; set; }
        public Uri? RepositoryUrl { get; set; }
        [Unicode]
        [MaxLength(BaGetDbContext.MaxRepositoryTypeLength)]
        public string RepositoryType { get; set; } = string.Empty;

        [Unicode]
        public List<string>? Tags { get; set; }

        /// <summary>
        /// Used for optimistic concurrency. TODO: Create an interceptors onto SaveChanges to handle this.
        /// </summary>
        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();
        
        [Column("Version")]
        [Unicode]
        [Required]
        [MaxLength(BaGetDbContext.MaxPackageVersionLength)]
        public string NormalizedVersionString { get; set; } = string.Empty;

        [Column("OriginalVersion")]
        [Unicode]
        [MaxLength(BaGetDbContext.MaxPackageVersionLength)]
        public string? OriginalVersionString { get; set; }
    }
}
