using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class HomeController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IMeterService _meterService;
    private readonly IBillingService _billingService;

    public HomeController(
        ICustomerService customerService,
        IMeterService meterService,
        IBillingService billingService)
    {
        _customerService = customerService;
        _meterService = meterService;
        _billingService = billingService;
    }

    public async Task<IActionResult> Index()
    {
        // Customer redirect to their own dashboard
        if (User.IsInRole("Customer"))
        {
            return RedirectToAction("Index", "Dashboard");
        }

        // Initialize ViewBag with default values
        ViewBag.TotalCustomers = 0;
        ViewBag.TotalMeters = 0;
        ViewBag.PendingBills = 0;
        ViewBag.MonthlyRevenue = 0m;
        ViewBag.RecentBills = new List<BillDto>();

        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var meters = await _meterService.GetAllMetersAsync();
            var bills = await _billingService.GetAllBillsAsync();
            
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
            TempData["Error"] = "Lỗi tải dữ liệu: " + ex.Message;
        }
        
        return View();
    }
}
