using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class DebtManagementController : Controller
{
    private readonly IDebtManagementService _debtManagementService;

    public DebtManagementController(IDebtManagementService debtManagementService)
    {
        _debtManagementService = debtManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool? isServiceActive = null)
    {
        try
        {
            var customers = await _debtManagementService.GetCustomersWithDebtAsync(isServiceActive);
            ViewBag.Filter = isServiceActive;
            return View(customers);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi tải dữ liệu: " + ex.Message;
            return View(new List<QL_HethongDiennuoc.Models.DTOs.DebtManagementDto>());
        }
    }

    [HttpPost("SuspendService/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendService(int id)
    {
        try
        {
            await _debtManagementService.SuspendServiceAsync(id);
            TempData["Success"] = "Đã cắt dịch vụ thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("RestoreService/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreService(int id)
    {
        try
        {
            await _debtManagementService.RestoreServiceAsync(id);
            TempData["Success"] = "Đã khôi phục dịch vụ thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
