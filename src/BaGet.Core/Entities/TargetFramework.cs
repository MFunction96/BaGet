using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BaGet.Core
{
    [Index(nameof(Moniker))]
    public class TargetFramework
    {
        [Key]
        public int Key { get; set; }
        [Unicode]
        [MaxLength(AbstractContext<DbContext>.MaxTargetFrameworkLength)]
        public string? Moniker { get; set; }

        public Package Package { get; set; } = null!;
    }
}
