using System.ComponentModel.DataAnnotations;

namespace QL_HethongDiennuoc.Models.Entities;

public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [StringLength(20)]
    public string? IdentityCard { get; set; }

    [StringLength(15)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    // User relationship (nullable for existing customers)
    public int? UserId { get; set; }
    public User? User { get; set; }

    // Navigation properties
    public ICollection<Meter> Meters { get; set; } = new List<Meter>();
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
}
