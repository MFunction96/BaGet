using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BaGet.Core.Entities
{
    public class BaGetDbContext(DbContextOptions<BaGetDbContext> options) : DbContext(options)
    {
        public const int MaxPackageIdLength = 128;
        public const int MaxPackageVersionLength = 64;
        public const int MaxPackageMinClientVersionLength = 44;
        public const int MaxPackageLanguageLength = 20;
        public const int MaxPackageTitleLength = 256;
        public const int MaxPackageTypeNameLength = 512;
        public const int MaxPackageTypeVersionLength = 64;
        public const int MaxRepositoryTypeLength = 100;
        public const int MaxTargetFrameworkLength = 256;

        public const int MaxPackageDependencyVersionRangeLength = 256;

        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageDependency> PackageDependencies { get; set; }
        public DbSet<PackageType> PackageTypes { get; set; }
        public DbSet<TargetFramework> TargetFrameworks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Package>(e =>
                e.HasIndex(p => new { p.Id, p.NormalizedVersionString })
                .IsUnique());

            builder.Entity<Package>()
                .HasMany(p => p.Dependencies)
                .WithOne(pd => pd.Package)
                .HasForeignKey(pd => pd.PackageKey)
                .IsRequired();

            builder.Entity<Package>()
                .HasMany(p => p.PackageTypes)
                .WithOne(pt => pt.Package)
                .HasForeignKey(pt => pt.PackageKey)
                .IsRequired();

            builder.Entity<Package>()
                .HasMany(p => p.TargetFrameworks)
                .WithOne(pt => pt.Package)
                .HasForeignKey(pt => pt.PackageKey)
                .IsRequired(false);
        }
    }
}
