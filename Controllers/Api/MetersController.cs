using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers.Api;

[ApiController]
[Route("api/meters")]
[Authorize]
public class ApiMetersController : ControllerBase
{
    private readonly IMeterService _meterService;

    public ApiMetersController(IMeterService meterService)
    {
        _meterService = meterService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var meters = await _meterService.GetAllMetersAsync();
        return Ok(meters);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var meter = await _meterService.GetMeterByIdAsync(id);
        if (meter == null)
            return NotFound(new { message = "Meter not found" });

        return Ok(meter);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        var meters = await _meterService.GetMetersByCustomerIdAsync(customerId);
        return Ok(meters);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateMeterDto dto)
    {
        try
        {
            var meter = await _meterService.CreateMeterAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = meter.Id }, meter);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMeterDto dto)
    {
        try
        {
            var meter = await _meterService.UpdateMeterAsync(id, dto);
            if (meter == null)
                return NotFound(new { message = "Meter not found" });

            return Ok(meter);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _meterService.DeleteMeterAsync(id);
        if (!result)
            return NotFound(new { message = "Meter not found" });

        return Ok(new { message = "Meter deleted successfully" });
    }
}
