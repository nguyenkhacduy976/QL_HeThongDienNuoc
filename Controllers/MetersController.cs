using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.ApiClients;
using QL_HethongDiennuoc.Utilities;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin")]
public class MetersController : Controller
{
    private readonly IApiClient _apiClient;

    public MetersController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var meters = await _apiClient.GetAsync<List<MeterDto>>("meters");
            return View(meters ?? new List<MeterDto>());
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError("Lỗi tải dữ liệu: " + ex.Message);
            return View(new List<MeterDto>());
        }
    }

    public async Task<IActionResult> Create()
    {
        await LoadCustomersToViewBag();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMeterDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadCustomersToViewBag();
            return View(dto);
        }

        try
        {
            await _apiClient.PostAsync<MeterDto>("meters", dto);
            TempData["Success"] = "Thêm công tơ thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError("Có lỗi xảy ra: " + ex.Message);
            await LoadCustomersToViewBag();
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var meter = await _apiClient.GetAsync<MeterDto>($"meters/{id}");

            if (meter == null)
            {
                TempData["Error"] = "Không tìm thấy công tơ!";
                return RedirectToAction(nameof(Index));
            }

            await LoadCustomersToViewBag();
            return View(meter);
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError("Lỗi: " + ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateMeterDto dto)
    {
        if (!ModelState.IsValid)
        {
            var meter = await _apiClient.GetAsync<MeterDto>($"meters/{id}");
            await LoadCustomersToViewBag();
            return View(meter);
        }

        try
        {
            await _apiClient.PutAsync<MeterDto>($"meters/{id}", dto);
            TempData["Success"] = "Cập nhật công tơ thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError("Có lỗi xảy ra: " + ex.Message);
            var meter = await _apiClient.GetAsync<MeterDto>($"meters/{id}");
            await LoadCustomersToViewBag();
            return View(meter);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _apiClient.DeleteAsync($"meters/{id}");
            TempData["Success"] = "Xóa công tơ thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError("Không thể xóa: " + ex.Message);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCustomersToViewBag()
    {
        var customers = await _apiClient.GetAsync<List<CustomerDto>>("customers");
        ViewBag.Customers = new SelectList(customers ?? new List<CustomerDto>(), "Id", "FullName");
    }
}
