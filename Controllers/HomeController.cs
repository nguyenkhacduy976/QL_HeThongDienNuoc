using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.ApiClients;
using QL_HethongDiennuoc.Utilities;

namespace QL_HethongDiennuoc.Controllers;

public class HomeController : Controller
{
    private readonly IApiClient _apiClient;

    public HomeController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        // Customer redirect to their own dashboard
        if (User.IsInRole("Customer"))
        {
            return RedirectToAction("Index", "Dashboard");
        }
        
        // Only Admin and Staff can view this page
        if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        // Initialize ViewBag with default values
        ViewBag.TotalCustomers = 0;
        ViewBag.TotalMeters = 0;
        ViewBag.PendingBills = 0;
        ViewBag.MonthlyRevenue = 0m;
        ViewBag.RecentBills = new List<BillDto>();

        try
        {
            var customers = await _apiClient.GetAsync<List<CustomerDto>>("customers");
            var meters = await _apiClient.GetAsync<List<MeterDto>>("meters");
            var bills = await _apiClient.GetAsync<List<BillDto>>("bills");
            
            customers ??= new List<CustomerDto>();
            meters ??= new List<MeterDto>();
            bills ??= new List<BillDto>();
            
            ViewBag.TotalCustomers = customers.Count;
            ViewBag.TotalMeters = meters.Count;
            ViewBag.PendingBills = bills.Count(b => b.Status != "Paid");
            
            var thisMonth = bills.Where(b => 
                b.IssueDate.Month == DateTime.Now.Month && 
                b.IssueDate.Year == DateTime.Now.Year
            ).ToList();
            ViewBag.MonthlyRevenue = thisMonth.Sum(b => b.PaidAmount);
            
            ViewBag.RecentBills = bills.OrderByDescending(b => b.IssueDate).Take(10).ToList();
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError("Lỗi tải dữ liệu: " + ex.Message);
        }
        
        return View();
    }
}
