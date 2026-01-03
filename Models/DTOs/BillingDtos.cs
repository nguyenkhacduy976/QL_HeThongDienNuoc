namespace QL_HethongDiennuoc.Models.DTOs;

public class ReadingDto
{
    public int Id { get; set; }
    public DateTime ReadingDate { get; set; }
    public decimal PreviousReading { get; set; }
    public decimal CurrentReading { get; set; }
    public decimal Consumption { get; set; }
    public string? Notes { get; set; }
    public int MeterId { get; set; }
    public string MeterNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string MeterType { get; set; } = string.Empty;
}

public class CreateReadingDto
{
    public int MeterId { get; set; }
    public DateTime ReadingDate { get; set; }
    public decimal CurrentReading { get; set; }
    public string? Notes { get; set; }
}

public class UpdateReadingDto
{
    public DateTime ReadingDate { get; set; }
    public decimal CurrentReading { get; set; }
    public string? Notes { get; set; }
}

public class BillDto
{
    public int Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Consumption { get; set; }
    public string ServiceType { get; set; } = string.Empty;
}

public class GenerateBillDto
{
    public int ReadingId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
    public int BillId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
}

public class CreatePaymentDto
{
    public int BillId { get; set; }
    public decimal Amount { get; set; }
    public int Method { get; set; } // 1=Cash, 2=BankTransfer, etc.
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
}
