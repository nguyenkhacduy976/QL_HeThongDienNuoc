using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class ServicesController : ControllerBase
{
    private readonly IServiceManagementService _serviceManagementService;

    public ServicesController(IServiceManagementService serviceManagementService)
    {
        _serviceManagementService = serviceManagementService;
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        var services = await _serviceManagementService.GetServicesByCustomerIdAsync(customerId);
        return Ok(services);
    }

    [HttpPost("suspend")]
    public async Task<IActionResult> SuspendService([FromBody] SuspendServiceDto dto)
    {
        try
        {
            var service = await _serviceManagementService.SuspendServiceAsync(dto);
            return Ok(service);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("restore")]
    public async Task<IActionResult> RestoreService([FromBody] RestoreServiceDto dto)
    {
        try
        {
            var service = await _serviceManagementService.RestoreServiceAsync(dto);
            return Ok(service);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
