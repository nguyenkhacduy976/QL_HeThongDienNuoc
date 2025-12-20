using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_HethongDiennuoc.Models.Entities;

public class Tariff
{
    [Key]
    public int Id { get; set; }

    [Required]
    public MeterType ServiceType { get; set; }

    [Required]
    public int Tier { get; set; } // Bậc 1, 2, 3...

    [Required]
    public decimal MinKwh { get; set; }

    public decimal? MaxKwh { get; set; } // null = unlimited

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerUnit { get; set; }

    public DateTime EffectiveDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    [StringLength(200)]
    public string? Description { get; set; }
}
