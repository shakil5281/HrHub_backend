using System;
using System.ComponentModel.DataAnnotations;

namespace ERPBackend.Core.DTOs
{
    public class ProductionLineDto
    {
        public int Id { get; set; }
        public int SL { get; set; }
        public string LineName { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class CreateProductionLineDto
    {
        [Required]
        public int SL { get; set; }

        [Required]
        [StringLength(100)]
        public string LineName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
    }
}
