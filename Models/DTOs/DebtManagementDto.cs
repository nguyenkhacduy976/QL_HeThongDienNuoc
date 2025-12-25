namespace QL_HethongDiennuoc.Models.DTOs;

public class DebtManagementDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal TotalDebt { get; set; }
    public int UnpaidBillsCount { get; set; }
    public DateTime? OldestUnpaidBillDate { get; set; }
    public bool IsServiceActive { get; set; }
    public DateTime? ServiceSuspendedDate { get; set; }
    public string ServiceStatus => IsServiceActive ? "Đang hoạt động" : "Đã cắt";
}

public class ServiceActionDto
{
    public int CustomerId { get; set; }
    public string Action { get; set; } = string.Empty; // "Suspend" or "Restore"
    public string? Notes { get; set; }
}
