using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ReadingsController : Controller
{
    private readonly IReadingService _readingService;
    private readonly IMeterService _meterService;

    public ReadingsController(IReadingService readingService, IMeterService meterService)
    {
        _readingService = readingService;
        _meterService = meterService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var readings = await _readingService.GetAllReadingsAsync();
            return View(readings);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi tải dữ liệu: " + ex.Message;
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
            var reading = await _readingService.CreateReadingAsync(dto);
            TempData["Success"] = $"Ghi chỉ số thành công! Tiêu thụ: {reading.Consumption:N0}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            await LoadMetersToViewBag();
            return View(dto);
        }
    }

    private async Task LoadMetersToViewBag()
    {
        var meters = await _meterService.GetAllMetersAsync();
        var meterItems = meters.Where(m => m.IsActive).Select(m => new SelectListItem
        {
            Value = m.Id.ToString(),
            Text = $"{m.MeterNumber} - {(m.Type == "Electric" ? "Điện" : "Nước")} - {m.CustomerName}"
        }).ToList();
        ViewBag.Meters = meterItems;
    }
}
