using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillsController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillsController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bills = await _billingService.GetAllBillsAsync();
        return Ok(bills);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var bill = await _billingService.GetBillByIdAsync(id);
        if (bill == null)
            return NotFound(new { message = "Bill not found" });

        return Ok(bill);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        var bills = await _billingService.GetBillsByCustomerIdAsync(customerId);
        return Ok(bills);
    }

    [HttpPost("generate")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GenerateBill([FromBody] GenerateBillDto dto)
    {
        try
        {
            var bill = await _billingService.GenerateBillAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = bill.Id }, bill);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBillStatusDto dto)
    {
        try
        {
            var bill = await _billingService.UpdateBillStatusAsync(id, (BillStatus)dto.Status);
            if (bill == null)
                return NotFound(new { message = "Bill not found" });

            return Ok(bill);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        try
        {
            var pdfBytes = await _billingService.GenerateBillPdfAsync(id);
            return File(pdfBytes, "application/pdf", $"bill_{id}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class UpdateBillStatusDto
{
    public int Status { get; set; }
}
