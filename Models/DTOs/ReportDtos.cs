namespace QL_HethongDiennuoc.Models.DTOs;

public class ConsumptionReportDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public List<ConsumptionItem> Items { get; set; } = new();
    public decimal TotalElectricConsumption { get; set; }
    public decimal TotalWaterConsumption { get; set; }
}

public class ConsumptionItem
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string MeterNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public decimal Consumption { get; set; }
    public DateTime ReadingDate { get; set; }
}

public class RevenueReportDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public int TotalBills { get; set; }
    public int PaidBills { get; set; }
    public int PendingBills { get; set; }
    public List<RevenueItem> Items { get; set; } = new();
}

public class RevenueItem
{
    public DateTime Month { get; set; }
    public decimal Revenue { get; set; }
    public decimal Paid { get; set; }
    public int BillCount { get; set; }
}

public class OutstandingBillDto
{
    public int BillId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
}
