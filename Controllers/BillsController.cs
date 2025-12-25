using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Helpers;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class BillsController : Controller
{
    private readonly IBillingService _billingService;
    private readonly IReadingService _readingService;
    private readonly ApplicationDbContext _context;

    public BillsController(IBillingService billingService, IReadingService readingService, ApplicationDbContext context)
    {
        _billingService = billingService;
        _readingService = readingService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var bills = await _billingService.GetAllBillsAsync();
            return View(bills);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi tải dữ liệu: " + ex.Message;
            return View(new List<BillDto>());
        }
    }

    public async Task<IActionResult> Generate(int? readingId = null)
    {
        await LoadUnbilledReadingsToViewBag(readingId);
        
        if (readingId.HasValue)
        {
            ViewBag.SelectedReadingId = readingId.Value;
        }
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(GenerateBillDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadUnbilledReadingsToViewBag(dto.ReadingId);
            return View(dto);
        }

        try
        {
            var bill = await _billingService.GenerateBillAsync(dto);
            TempData["Success"] = $"Tạo hóa đơn thành công! Số hóa đơn: {bill.BillNumber}, Số tiền: {bill.Amount:N0} đ";
            return RedirectToAction(nameof(Details), new { id = bill.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            await LoadUnbilledReadingsToViewBag(dto.ReadingId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var bill = await _context.Bills
                .Include(b => b.Customer)
                .Include(b => b.Reading)
                    .ThenInclude(r => r.Meter)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn!";
                return RedirectToAction(nameof(Index));
            }

            // Get tier breakdown
            var tariffs = await _context.Tariffs
                .Where(t => t.ServiceType == bill.Reading.Meter.Type && t.IsActive)
                .ToListAsync();
            
            var breakdown = TariffCalculator.GetTierBreakdown(bill.Reading.Consumption, bill.Reading.Meter.Type, tariffs);
            ViewBag.TierBreakdown = breakdown;

            return View(bill);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task LoadUnbilledReadingsToViewBag(int? selectedReadingId = null)
    {
        // Get readings that don't have a bill yet
        var unbilledReadings = await _context.Readings
            .Include(r => r.Meter)
                .ThenInclude(m => m.Customer)
            .Where(r => !_context.Bills.Any(b => b.ReadingId == r.Id))
            .OrderByDescending(r => r.ReadingDate)
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.Meter.MeterNumber} - {r.Meter.Customer.FullName} - {r.ReadingDate:dd/MM/yyyy} - Tiêu thụ: {r.Consumption:N0}",
                Selected = r.Id == selectedReadingId
            })
            .ToListAsync();

        ViewBag.UnbilledReadings = unbilledReadings;
    }
}
