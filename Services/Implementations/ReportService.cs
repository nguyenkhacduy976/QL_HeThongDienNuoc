using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Services.Implementations;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConsumptionReportDto> GetConsumptionReportAsync(DateTime? startDate, DateTime? endDate, int? customerId)
    {
        var query = _context.Readings
            .Include(r => r.Meter)
                .ThenInclude(m => m.Customer)
            .AsQueryable();

        // Apply filters
        if (startDate.HasValue)
            query = query.Where(r => r.ReadingDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.ReadingDate <= endDate.Value);

        if (customerId.HasValue)
            query = query.Where(r => r.Meter.CustomerId == customerId.Value);

        var readings = await query.ToListAsync();

        var items = readings.Select(r => new ConsumptionItem
        {
            CustomerId = r.Meter.CustomerId,
            CustomerName = r.Meter.Customer.FullName,
            MeterNumber = r.Meter.MeterNumber,
            ServiceType = r.Meter.Type.ToString(),
            Consumption = r.Consumption,
            ReadingDate = r.ReadingDate
        }).ToList();

        var report = new ConsumptionReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            CustomerId = customerId,
            CustomerName = customerId.HasValue ? items.FirstOrDefault()?.CustomerName : null,
            Items = items,
            TotalElectricConsumption = items.Where(i => i.ServiceType == "Electric").Sum(i => i.Consumption),
            TotalWaterConsumption = items.Where(i => i.ServiceType == "Water").Sum(i => i.Consumption)
        };

        return report;
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Bills.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(b => b.IssueDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(b => b.IssueDate <= endDate.Value);

        var bills = await query.ToListAsync();

        // Group by month
        var monthlyRevenue = bills
            .GroupBy(b => new { b.IssueDate.Year, b.IssueDate.Month })
            .Select(g => new RevenueItem
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                Revenue = g.Sum(b => b.Amount),
                Paid = g.Sum(b => b.PaidAmount),
                BillCount = g.Count()
            })
            .OrderBy(r => r.Month)
            .ToList();

        var report = new RevenueReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = bills.Sum(b => b.Amount),
            TotalPaid = bills.Sum(b => b.PaidAmount),
            TotalPending = bills.Where(b => b.Status != BillStatus.Paid).Sum(b => b.Amount - b.PaidAmount),
            TotalBills = bills.Count,
            PaidBills = bills.Count(b => b.Status == BillStatus.Paid),
            PendingBills = bills.Count(b => b.Status != BillStatus.Paid),
            Items = monthlyRevenue
        };

        return report;
    }

    public async Task<List<OutstandingBillDto>> GetOutstandingBillsAsync()
    {
        var today = DateTime.Now;

        // First, get the bills without calculating DaysOverdue in SQL
        var bills = await _context.Bills
            .Include(b => b.Customer)
            .Where(b => b.Status != BillStatus.Paid && b.DueDate < today)
            .Select(b => new OutstandingBillDto
            {
                BillId = b.Id,
                BillNumber = b.BillNumber,
                CustomerId = b.CustomerId,
                CustomerName = b.Customer.FullName,
                PhoneNumber = b.Customer.PhoneNumber ?? "",
                Email = b.Customer.Email ?? "",
                Amount = b.Amount,
                PaidAmount = b.PaidAmount,
                OutstandingAmount = b.Amount - b.PaidAmount,
                IssueDate = b.IssueDate,
                DueDate = b.DueDate,
                DaysOverdue = 0 // Will be calculated below
            })
            .ToListAsync();

        // Calculate DaysOverdue client-side and sort
        foreach (var bill in bills)
        {
            bill.DaysOverdue = (int)(today - bill.DueDate).TotalDays;
        }

        return bills.OrderByDescending(b => b.DaysOverdue).ToList();
    }
}
