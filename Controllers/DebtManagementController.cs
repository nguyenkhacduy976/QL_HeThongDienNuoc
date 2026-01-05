using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.ApiClients;
using QL_HethongDiennuoc.Utilities;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class DebtManagementController : Controller
{
    private readonly IApiClient _apiClient;

    public DebtManagementController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool? isServiceActive = null)
    {
        try
        {
            var url = "debt-management/customers";
            if (isServiceActive.HasValue)
            {
                url += $"?isServiceActive={isServiceActive.Value}";
            }
            
            var customers = await _apiClient.GetAsync<List<DebtManagementDto>>(url);
            ViewBag.Filter = isServiceActive;
            return View(customers ?? new List<DebtManagementDto>());
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError("Lỗi tải dữ liệu: " + ex.Message);
            return View(new List<DebtManagementDto>());
        }
    }

    [HttpPost("SuspendService/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendService(int id)
    {
        try
        {
            await _apiClient.PostAsync<object>($"debt-management/suspend/{id}", new { });
            TempData["Success"] = "Đã cắt dịch vụ thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError(ex.Message);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("RestoreService/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreService(int id)
    {
        try
        {
            await _apiClient.PostAsync<object>($"debt-management/restore/{id}", new { });
            TempData["Success"] = "Đã khôi phục dịch vụ thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
