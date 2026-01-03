using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers.Api;

[ApiController]
[Route("api/readings")]
[Authorize(AuthenticationSchemes = "Bearer,Cookies")]
public class ApiReadingsController : ControllerBase
{
    private readonly IReadingService _readingService;

    public ApiReadingsController(IReadingService readingService)
    {
        _readingService = readingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var readings = await _readingService.GetAllReadingsAsync();
        return Ok(readings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reading = await _readingService.GetReadingByIdAsync(id);
        if (reading == null)
            return NotFound(new { message = "Reading not found" });

        return Ok(reading);
    }

    [HttpGet("meter/{meterId}")]
    public async Task<IActionResult> GetByMeterId(int meterId)
    {
        var readings = await _readingService.GetReadingsByMeterIdAsync(meterId);
        return Ok(readings);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateReadingDto dto)
    {
        try
        {
            var reading = await _readingService.CreateReadingAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = reading.Id }, reading);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReadingDto dto)
    {
        try
        {
            var reading = await _readingService.UpdateReadingAsync(id, dto);
            if (reading == null)
                return NotFound(new { message = "Không tìm thấy chỉ số cần sửa" });

            return Ok(reading);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _readingService.DeleteReadingAsync(id);
            if (!result)
                return NotFound(new { message = "Không tìm thấy chỉ số cần xóa" });

            return Ok(new { message = "Đã xóa chỉ số thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
