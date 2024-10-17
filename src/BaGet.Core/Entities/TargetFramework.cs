using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using BaGet.Core.Entities;

namespace BaGet.Core
{
    [Index(nameof(Moniker))]
    public class TargetFramework
    {
        [Key]
        public int Key { get; set; }
        [Unicode]
        [MaxLength(BaGetDbContext.MaxTargetFrameworkLength)]
        public string? Moniker { get; set; }

        public Package Package { get; set; } = null!;
    }
}
