using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.Interfaces;
using System.Security.Claims;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Customer")]
public class DashboardController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IBillingService _billingService;

    public DashboardController(
        ICustomerService customerService,
        IBillingService billingService)
    {
        _customerService = customerService;
        _billingService = billingService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalBills = 0;
        ViewBag.UnpaidBills = 0;
        ViewBag.TotalDebt = 0m;
        ViewBag.RecentBills = new List<BillDto>();

        try
        {
            // Get current user's username from claims
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            if (!string.IsNullOrEmpty(username))
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var customer = customers.FirstOrDefault(c => c.FullName == username);
                
                if (customer != null)
                {
                    var bills = await _billingService.GetBillsByCustomerIdAsync(customer.Id);
                    
                    ViewBag.TotalBills = bills.Count;
                    ViewBag.UnpaidBills = bills.Count(b => b.Status != "Paid");
                    ViewBag.TotalDebt = bills.Where(b => b.Status != "Paid").Sum(b => b.Amount - b.PaidAmount);
                    ViewBag.RecentBills = bills.OrderByDescending(b => b.IssueDate).Take(5).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi tải dữ liệu: " + ex.Message;
        }

        return View();
    }
}
