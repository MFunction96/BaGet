using System.ComponentModel.DataAnnotations;

namespace BaGet.Core.Configuration
{
    public class DatabaseOptions
    {
        [Required]
        public required string Type { get; set; }

        [Required]
        public required string ConnectionString { get; set; }
    }
}
