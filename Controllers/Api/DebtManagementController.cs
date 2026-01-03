using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers.Api;

[ApiController]
[Route("api/debt-management")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class ApiDebtManagementController : ControllerBase
{
    private readonly IDebtManagementService _debtManagementService;

    public ApiDebtManagementController(IDebtManagementService debtManagementService)
    {
        _debtManagementService = debtManagementService;
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomersWithDebt([FromQuery] bool? isServiceActive = null)
    {
        var customers = await _debtManagementService.GetCustomersWithDebtAsync(isServiceActive);
        return Ok(customers);
    }

    [HttpPost("suspend/{customerId}")]
    public async Task<IActionResult> SuspendService(int customerId, [FromBody] ServiceActionDto? dto = null)
    {
        var success = await _debtManagementService.SuspendServiceAsync(customerId, dto?.Notes);
        
        if (!success)
            return BadRequest(new { message = "Không thể cắt dịch vụ. Khách hàng không tồn tại hoặc dịch vụ đã bị cắt." });

        return Ok(new { message = "Dịch vụ đã được cắt thành công." });
    }

    [HttpPost("restore/{customerId}")]
    public async Task<IActionResult> RestoreService(int customerId, [FromBody] ServiceActionDto? dto = null)
    {
        var success = await _debtManagementService.RestoreServiceAsync(customerId, dto?.Notes);
        
        if (!success)
            return BadRequest(new { message = "Không thể khôi phục dịch vụ. Khách hàng không tồn tại hoặc dịch vụ đang hoạt động." });

        return Ok(new { message = "Dịch vụ đã được khôi phục thành công." });
    }
}
