using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.ApiClients;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ReportsController : Controller
{
    private readonly IApiClient _apiClient;

    public ReportsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        // Calculate quick statistics for the dashboard
        var today = DateTime.Now;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        
        try
        {
            var revenueReport = await _apiClient.GetAsync<RevenueReportDto>($"reports/revenue?startDate={firstDayOfMonth:yyyy-MM-dd}");
            var outstandingBills = await _apiClient.GetAsync<List<BillDto>>("reports/outstanding");
            
            ViewBag.TotalElectric = 0m; // Would need consumption report API
            ViewBag.TotalWater = 0m;
            ViewBag.MonthlyRevenue = revenueReport?.TotalRevenue ?? 0m;
            ViewBag.OutstandingDebt = (outstandingBills ?? new List<BillDto>()).Sum(b => b.Amount - b.PaidAmount);
        }
        catch
        {
            ViewBag.TotalElectric = 0m;
            ViewBag.TotalWater = 0m;
            ViewBag.MonthlyRevenue = 0m;
            ViewBag.OutstandingDebt = 0m;
        }
        
        return View();
    }
    
    public async Task<IActionResult> Consumption(DateTime? startDate, DateTime? endDate, int? customerId)
    {
        // Load customers for dropdown
        var customers = await _apiClient.GetAsync<List<CustomerDto>>("customers");
        ViewBag.Customers = new SelectList(
            (customers ?? new List<CustomerDto>()).OrderBy(c => c.FullName),
            "Id",
            "FullName"
        );
        
        var url = "reports/consumption";
        var queryParams = new List<string>();
        if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        if (customerId.HasValue) queryParams.Add($"customerId={customerId.Value}");
        if (queryParams.Any()) url += "?" + string.Join("&", queryParams);
        
        var report = await _apiClient.GetAsync<ConsumptionReportDto>(url);
        return View(report ?? new ConsumptionReportDto());
    }
    
    public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate)
    {
        var url = "reports/revenue";
        var queryParams = new List<string>();
        if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        if (queryParams.Any()) url += "?" + string.Join("&", queryParams);
        
        var report = await _apiClient.GetAsync<RevenueReportDto>(url);
        return View(report ?? new RevenueReportDto());
    }
    
    public async Task<IActionResult> Outstanding()
    {
        var bills = await _apiClient.GetAsync<List<BillDto>>("reports/outstanding");
        return View(bills ?? new List<BillDto>());
    }
}
