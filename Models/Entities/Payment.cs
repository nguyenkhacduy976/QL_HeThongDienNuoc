using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_HethongDiennuoc.Models.Entities;

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    Card = 3,
    Momo = 4,
    Other = 5
}

public class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public PaymentMethod Method { get; set; }

    [StringLength(100)]
    public string? TransactionId { get; set; }

    [StringLength(200)]
    public string? Notes { get; set; }

    // Foreign key
    [Required]
    public int BillId { get; set; }

    // Navigation property
    [ForeignKey(nameof(BillId))]
    public Bill Bill { get; set; } = null!;
}
