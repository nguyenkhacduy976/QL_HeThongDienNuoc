using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly ApplicationDbContext _context;

    public ReportsController(IReportService reportService, ApplicationDbContext context)
    {
        _reportService = reportService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Calculate quick statistics for the dashboard
        var today = DateTime.Now;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        
        // Total consumption (all time)
        var totalElectric = await _context.Readings
            .Where(r => r.Meter.Type == Models.Entities.MeterType.Electric)
            .SumAsync(r => r.Consumption);
            
        var totalWater = await _context.Readings
            .Where(r => r.Meter.Type == Models.Entities.MeterType.Water)
            .SumAsync(r => r.Consumption);
        
        // Monthly revenue
        var monthlyRevenue = await _context.Bills
            .Where(b => b.IssueDate >= firstDayOfMonth)
            .SumAsync(b => b.Amount);
        
        // Outstanding debt
        var outstandingDebt = await _context.Bills
            .Where(b => b.Status != Models.Entities.BillStatus.Paid && b.DueDate < today)
            .SumAsync(b => b.Amount - b.PaidAmount);
        
        ViewBag.TotalElectric = totalElectric;
        ViewBag.TotalWater = totalWater;
        ViewBag.MonthlyRevenue = monthlyRevenue;
        ViewBag.OutstandingDebt = outstandingDebt;
        
        return View();
    }
    
    public async Task<IActionResult> Consumption(DateTime? startDate, DateTime? endDate, int? customerId)
    {
        // Load customers for dropdown
        ViewBag.Customers = new SelectList(
            await _context.Customers.OrderBy(c => c.FullName).ToListAsync(),
            "Id",
            "FullName"
        );
        
        var report = await _reportService.GetConsumptionReportAsync(startDate, endDate, customerId);
        return View(report);
    }
    
    public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate)
    {
        var report = await _reportService.GetRevenueReportAsync(startDate, endDate);
        return View(report);
    }
    
    public async Task<IActionResult> Outstanding()
    {
        var bills = await _reportService.GetOutstandingBillsAsync();
        return View(bills);
    }
}
