using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.ApiClients;
using QL_HethongDiennuoc.Utilities;
using System.Security.Claims;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Customer")]
public class DashboardController : Controller
{
    private readonly IApiClient _apiClient;

    public DashboardController(IApiClient apiClient)
    {
        _apiClient = apiClient;
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
                var customers = await _apiClient.GetAsync<List<CustomerDto>>("customers");
                var customer = (customers ?? new List<CustomerDto>()).FirstOrDefault(c => c.UserId == userId);
                
                if (customer != null)
                {
                    ViewBag.CustomerName = customer.FullName;
                    
                    // Get bills for this customer
                    var bills = await _apiClient.GetAsync<List<BillDto>>($"bills/customer/{customer.Id}");
                    bills ??= new List<BillDto>();
                    
                    ViewBag.TotalBills = bills.Count;
                    ViewBag.UnpaidBills = bills.Count(b => b.Status != "Paid");
                    ViewBag.TotalDebt = bills.Where(b => b.Status != "Paid").Sum(b => b.Amount - b.PaidAmount);
                    ViewBag.MyBills = bills.OrderByDescending(b => b.IssueDate).Take(5).ToList();
                    
                    // Get meters for this customer
                    var allMeters = await _apiClient.GetAsync<List<MeterDto>>($"meters/customer/{customer.Id}");
                    var customerMeters = allMeters ?? new List<MeterDto>();
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
            TempData["Error"] = MessageHelper.GetUserFriendlyError("Lỗi tải dữ liệu: " + ex.Message);
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
            var customers = await _apiClient.GetAsync<List<CustomerDto>>("customers");
            var customer = (customers ?? new List<CustomerDto>()).FirstOrDefault(c => c.UserId == userId);
            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng" });
            }

            // Get bill and verify ownership
            var bills = await _apiClient.GetAsync<List<BillDto>>($"bills/customer/{customer.Id}");
            var bill = (bills ?? new List<BillDto>()).FirstOrDefault(b => b.Id == billId);
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

            // Create payment via API
            var paymentDto = new CreatePaymentDto
            {
                BillId = billId,
                Amount = remainingAmount,
                Method = paymentMethod,
                Notes = $"Thanh toán từ khách hàng {customer.FullName}"
            };

            var payment = await _apiClient.PostAsync<PaymentDto>("payments", paymentDto);

            TempData["Success"] = $"Thanh toán thành công! Số tiền: {remainingAmount:N0} đ";
            return Json(new { success = true, message = "Thanh toán thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }
}
