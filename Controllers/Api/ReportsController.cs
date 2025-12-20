using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Báo cáo tiêu thụ điện nước
    /// </summary>
    [HttpGet("consumption")]
    public async Task<IActionResult> GetConsumptionReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? customerId)
    {
        try
        {
            var report = await _reportService.GetConsumptionReportAsync(startDate, endDate, customerId);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Báo cáo doanh thu
    /// </summary>
    [HttpGet("revenue")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetRevenueReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var report = await _reportService.GetRevenueReportAsync(startDate, endDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Danh sách hóa đơn nợ quá hạn
    /// </summary>
    [HttpGet("outstanding")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetOutstandingBills()
    {
        try
        {
            var bills = await _reportService.GetOutstandingBillsAsync();
            return Ok(bills);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
