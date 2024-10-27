using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BaGet.Core.Entities
{
    [Index(nameof(Moniker))]
    public class TargetFramework : IEntity
    {
        [Key]
        public int Key { get; set; }

        [Unicode]
        [MaxLength(BaGetDbContext.MaxTargetFrameworkLength)]
        public string? Moniker { get; set; }

        public int? PackageKey { get; set; }

        public Package? Package { get; set; }
    }
}
