using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin")]
public class MetersController : Controller
{
    private readonly IMeterService _meterService;
    private readonly ICustomerService _customerService;

    public MetersController(IMeterService meterService, ICustomerService customerService)
    {
        _meterService = meterService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var meters = await _meterService.GetAllMetersAsync();
            return View(meters);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi tải dữ liệu: " + ex.Message;
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
            await _meterService.CreateMeterAsync(dto);
            TempData["Success"] = "Thêm công tơ thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            await LoadCustomersToViewBag();
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var meter = await _meterService.GetMeterByIdAsync(id);

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
            TempData["Error"] = "Lỗi: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateMeterDto dto)
    {
        if (!ModelState.IsValid)
        {
            var meter = await _meterService.GetMeterByIdAsync(id);
            await LoadCustomersToViewBag();
            return View(meter);
        }

        try
        {
            await _meterService.UpdateMeterAsync(id, dto);
            TempData["Success"] = "Cập nhật công tơ thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            var meter = await _meterService.GetMeterByIdAsync(id);
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
            await _meterService.DeleteMeterAsync(id);
            TempData["Success"] = "Xóa công tơ thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCustomersToViewBag()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        ViewBag.Customers = new SelectList(customers, "Id", "FullName");
    }
}
