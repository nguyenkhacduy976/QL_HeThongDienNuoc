using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_HethongDiennuoc.Models.Entities;

public enum MeterType
{
    Electric = 1,
    Water = 2
}

public class Meter
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string MeterNumber { get; set; } = string.Empty;

    [Required]
    public MeterType Type { get; set; }

    public DateTime InstallDate { get; set; } = DateTime.Now;

    [StringLength(100)]
    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;

    // Foreign key
    [Required]
    public int CustomerId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = null!;

    public ICollection<Reading> Readings { get; set; } = new List<Reading>();
}
