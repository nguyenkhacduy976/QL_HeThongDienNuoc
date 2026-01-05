using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.ApiClients;
using QL_HethongDiennuoc.Utilities;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ReadingsController : Controller
{
    private readonly IApiClient _apiClient;

    public ReadingsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var readings = await _apiClient.GetAsync<List<ReadingDto>>("readings");
            return View(readings ?? new List<ReadingDto>());
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError("Lỗi tải dữ liệu: " + ex.Message);
            return View(new List<ReadingDto>());
        }
    }

    public async Task<IActionResult> Create()
    {
        await LoadMetersToViewBag();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReadingDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadMetersToViewBag();
            return View(dto);
        }

        try
        {
            var reading = await _apiClient.PostAsync<ReadingDto>("readings", dto);
            if (reading != null)
            {
                TempData["Success"] = $"Ghi chỉ số thành công! Tiêu thụ: {reading.Consumption:N0}";
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError("Có lỗi xảy ra: " + ex.Message);
            await LoadMetersToViewBag();
            return View(dto);
        }
    }

    private async Task LoadMetersToViewBag()
    {
        var meters = await _apiClient.GetAsync<List<MeterDto>>("meters");
        var meterItems = (meters ?? new List<MeterDto>())
            .Where(m => m.IsActive)
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.MeterNumber} - {(m.Type == "Electric" ? "Điện" : "Nước")} - {m.CustomerName}"
            }).ToList();
        ViewBag.Meters = meterItems;
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var reading = await _apiClient.GetAsync<ReadingDto>($"readings/{id}");
            if (reading == null)
            {
                TempData["Error"] = "Không tìm thấy chỉ số!";
                return RedirectToAction(nameof(Index));
            }
            
            var dto = new UpdateReadingDto
            {
                ReadingDate = reading.ReadingDate,
                CurrentReading = reading.CurrentReading,
                Notes = reading.Notes
            };
            
            ViewBag.Reading = reading;
            return View(dto);
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateReadingDto dto)
    {
        if (!ModelState.IsValid)
        {
            var reading = await _apiClient.GetAsync<ReadingDto>($"readings/{id}");
            ViewBag.Reading = reading;
            return View(dto);
        }

        try
        {
            var result = await _apiClient.PutAsync<ReadingDto>($"readings/{id}", dto);
            if (result == null)
            {
                TempData["Error"] = "Không tìm thấy chỉ số cần sửa!";
            }
            else
            {
                TempData["Success"] = "Đã cập nhật chỉ số thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            var reading = await _apiClient.GetAsync<ReadingDto>($"readings/{id}");
            ViewBag.Reading = reading;
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiClient.DeleteAsync($"readings/{id}");
            if (result)
            {
                TempData["Success"] = "Đã xóa chỉ số thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy chỉ số cần xóa!";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = MessageHelper.GetUserFriendlyError(ex.Message);
        }
        return RedirectToAction(nameof(Index));
    }
}
