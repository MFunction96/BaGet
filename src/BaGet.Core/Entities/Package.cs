using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace BaGet.Core
{
    // See NuGetGallery's: https://github.com/NuGet/NuGetGallery/blob/master/src/NuGetGallery.Core/Entities/Package.cs
    [Index(nameof(Id))]
    public class Package
    {
        [Key]
        public int Key { get; set; }

        [Required]
        [Unicode]
        [MaxLength(AbstractContext<DbContext>.MaxPackageIdLength)]
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
        public ICollection<string> Authors { get; set; } = new List<string>();

        [Unicode]
        [Column(TypeName = "text")]
        public string Description { get; set; } = string.Empty;
        public long Downloads { get; set; }
        public bool HasReadme { get; set; }
        public bool HasEmbeddedIcon { get; set; }
        public bool IsPrerelease { get; set; }

        [Unicode]
        [Column(TypeName = "text")]
        public string? ReleaseNotes { get; set; }
        [Unicode]
        [MaxLength(AbstractContext<DbContext>.MaxPackageLanguageLength)]
        public string Language { get; set; } = string.Empty;
        public bool Listed { get; set; }
        [Unicode]
        [MaxLength(AbstractContext<DbContext>.MaxPackageMinClientVersionLength)]
        public string MinClientVersion { get; set; } = string.Empty;
        public DateTime Published { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public SemVerLevel SemVerLevel { get; set; }
        [Unicode]
        [Column(TypeName = "text")]
        public string Summary { get; set; } = string.Empty;
        [Unicode]
        [MaxLength(AbstractContext<DbContext>.MaxPackageTitleLength)]
        public string Title { get; set; } = string.Empty;

        [Unicode]
        [Column(TypeName = "text")]
        public string? IconUrl { get; set; }
        [Unicode]
        [Column(TypeName = "text")]
        public string? LicenseUrl { get; set; }
        [Unicode]
        [Column(TypeName = "text")]
        public string? ProjectUrl { get; set; }
        [Unicode]
        [Column(TypeName = "text")]
        public string? RepositoryUrl { get; set; }
        [Unicode]
        [MaxLength(AbstractContext<DbContext>.MaxRepositoryTypeLength)]
        public string RepositoryType { get; set; } = string.Empty;

        [Unicode]
        public ICollection<string>? Tags { get; set; }

        /// <summary>
        /// Used for optimistic concurrency.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
        
        [Column("Version")]
        [Unicode]
        [Required]
        [MaxLength(AbstractContext<DbContext>.MaxPackageVersionLength)]
        public string NormalizedVersionString { get; set; } = string.Empty;

        [Column("OriginalVersion")]
        [Unicode]
        [MaxLength(AbstractContext<DbContext>.MaxPackageVersionLength)]
        public string? OriginalVersionString { get; set; }

        public ICollection<PackageDependency> Dependencies { get; set; } = new List<PackageDependency>();
        public ICollection<PackageType> PackageTypes { get; set; } = new List<PackageType>();
        public ICollection<TargetFramework> TargetFrameworks { get; set; } = new List<TargetFramework>();
    }
}
