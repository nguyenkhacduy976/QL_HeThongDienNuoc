using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_HethongDiennuoc.Models.Entities;

public class Reading
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime ReadingDate { get; set; }

    [Required]
    public decimal PreviousReading { get; set; }

    [Required]
    public decimal CurrentReading { get; set; }

    [Required]
    public decimal Consumption { get; set; }

    [StringLength(200)]
    public string? Notes { get; set; }

    // Foreign key
    [Required]
    public int MeterId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(MeterId))]
    public Meter Meter { get; set; } = null!;

    public Bill? Bill { get; set; }
}
