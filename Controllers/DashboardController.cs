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
    private readonly IMeterService _meterService;
    private readonly IPaymentService _paymentService;

    public DashboardController(
        ICustomerService customerService,
        IBillingService billingService,
        IMeterService meterService,
        IPaymentService paymentService)
    {
        _customerService = customerService;
        _billingService = billingService;
        _meterService = meterService;
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Index()
    {
        // Initialize ViewBag with default values
        ViewBag.CustomerName = "";
        ViewBag.TotalBills = 0;
        ViewBag.UnpaidBills = 0;
        ViewBag.TotalDebt = 0m;
        ViewBag.MyBills = new List<BillDto>();
        ViewBag.Meters = new List<MeterDto>();
        ViewBag.ElectricMeter = (MeterDto?)null;
        ViewBag.WaterMeter = (MeterDto?)null;
        ViewBag.MonthlyConsumption = new Dictionary<string, decimal>();

        try
        {
            // Get current user's ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                // Find customer by UserId
                var customers = await _customerService.GetAllCustomersAsync();
                var customer = customers.FirstOrDefault(c => c.UserId == userId);
                
                if (customer != null)
                {
                    ViewBag.CustomerName = customer.FullName;
                    
                    // Get bills for this customer
                    var bills = await _billingService.GetBillsByCustomerIdAsync(customer.Id);
                    
                    ViewBag.TotalBills = bills.Count;
                    ViewBag.UnpaidBills = bills.Count(b => b.Status != "Paid");
                    ViewBag.TotalDebt = bills.Where(b => b.Status != "Paid").Sum(b => b.Amount - b.PaidAmount);
                    ViewBag.MyBills = bills.OrderByDescending(b => b.IssueDate).Take(5).ToList();
                    
                    // Get meters for this customer
                    var allMeters = await _meterService.GetAllMetersAsync();
                    var customerMeters = allMeters.Where(m => m.CustomerId == customer.Id).ToList();
                    ViewBag.Meters = customerMeters;
                    
                    // Separate electric and water meters for easy access
                    ViewBag.ElectricMeter = customerMeters.FirstOrDefault(m => m.Type == "Electric");
                    ViewBag.WaterMeter = customerMeters.FirstOrDefault(m => m.Type == "Water");
                    
                    // Calculate monthly consumption for the last 6 months
                    var monthlyData = new Dictionary<string, decimal>();
                    var startDate = DateTime.Now.AddMonths(-6);
                    
                    foreach (var bill in bills.Where(b => b.IssueDate >= startDate).OrderBy(b => b.IssueDate))
                    {
                        var monthKey = bill.IssueDate.ToString("MM/yyyy");
                        if (!monthlyData.ContainsKey(monthKey))
                        {
                            monthlyData[monthKey] = 0;
                        }
                        monthlyData[monthKey] += bill.Consumption;
                    }
                    
                    ViewBag.MonthlyConsumption = monthlyData;
                }
                else
                {
                    TempData["Warning"] = "Không tìm thấy thông tin khách hàng liên kết với tài khoản này.";
                }
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi tải dữ liệu: " + ex.Message;
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayBill(int billId, int paymentMethod)
    {
        try
        {
            // Get current user's ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Không xác định được người dùng" });
            }

            // Get customer
            var customers = await _customerService.GetAllCustomersAsync();
            var customer = customers.FirstOrDefault(c => c.UserId == userId);
            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng" });
            }

            // Get bill and verify ownership
            var bills = await _billingService.GetBillsByCustomerIdAsync(customer.Id);
            var bill = bills.FirstOrDefault(b => b.Id == billId);
            if (bill == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hóa đơn hoặc bạn không có quyền thanh toán hóa đơn này" });
            }

            // Check if bill is already paid
            if (bill.Status == "Paid")
            {
                return Json(new { success = false, message = "Hóa đơn này đã được thanh toán" });
            }

            // Calculate remaining amount
            var remainingAmount = bill.Amount - bill.PaidAmount;

            // Create payment
            var paymentDto = new CreatePaymentDto
            {
                BillId = billId,
                Amount = remainingAmount,
                Method = paymentMethod,
                Notes = $"Thanh toán từ khách hàng {customer.FullName}"
            };

            var payment = await _paymentService.CreatePaymentAsync(paymentDto);

            TempData["Success"] = $"Thanh toán thành công! Số tiền: {remainingAmount:N0} đ";
            return Json(new { success = true, message = "Thanh toán thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }
}
