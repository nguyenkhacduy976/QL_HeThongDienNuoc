using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_HethongDiennuoc.Models.Entities;

public enum ServiceType
{
    Electric = 1,
    Water = 2
}

public enum ServiceStatus
{
    Active = 1,
    Suspended = 2,
    Terminated = 3
}

public class Service
{
    [Key]
    public int Id { get; set; }

    [Required]
    public ServiceType Type { get; set; }

    [Required]
    public ServiceStatus Status { get; set; } = ServiceStatus.Active;

    public DateTime? SuspendDate { get; set; }

    public DateTime? RestoreDate { get; set; }

    [StringLength(200)]
    public string? Reason { get; set; }

    // Foreign key
    [Required]
    public int CustomerId { get; set; }

    // Navigation property
    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = null!;
}
