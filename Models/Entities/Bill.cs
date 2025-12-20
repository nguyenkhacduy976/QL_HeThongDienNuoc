using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_HethongDiennuoc.Models.Entities;

public enum BillStatus
{
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Partial = 4
}

public class Bill
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string BillNumber { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime IssueDate { get; set; } = DateTime.Now;

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public BillStatus Status { get; set; } = BillStatus.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; } = 0;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Foreign keys
    [Required]
    public int ReadingId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ReadingId))]
    public Reading Reading { get; set; } = null!;

    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = null!;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
