using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers.Api;

[ApiController]
[Route("api/bills")]
[Authorize(AuthenticationSchemes = "Bearer,Cookies")]
public class ApiBillsController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly INotificationService _notificationService;

    public ApiBillsController(
        IBillingService billingService,
        INotificationService notificationService)
    {
        _billingService = billingService;
        _notificationService = notificationService;
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

    [HttpPost("{id}/send-notification")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> SendNotification(int id)
    {
        try
        {
            var bill = await _billingService.GetBillByIdAsync(id);
            if (bill == null)
                return NotFound(new { message = "Bill not found" });

            await _notificationService.SendBillNotificationAsync(id);
            
            return Ok(new 
            { 
                message = "Notification sent successfully", 
                billId = id,
                billNumber = bill.BillNumber
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("send-bulk-notifications")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> SendBulkNotifications()
    {
        try
        {
            var bills = await _billingService.GetAllBillsAsync();
            // Exclude paid bills
            var unpaidBills = bills.Where(b => b.Status != "Paid").ToList();

            int notificationsSent = 0;
            var errors = new List<string>();

            foreach (var bill in unpaidBills)
            {
                try
                {
                    // Check bill status and send appropriate notification
                    if (bill.DueDate < DateTime.Now)
                    {
                        // Overdue: Send urgent reminder
                        var daysOverdue = (DateTime.Now - bill.DueDate).Days;
                        await _notificationService.SendPaymentOverdueReminderAsync(bill.Id, daysOverdue);
                    }
                    else if ((bill.DueDate - DateTime.Now).Days <= 3)
                    {
                        // Due soon (within 3 days): Send due reminder
                        await _notificationService.SendPaymentDueReminderAsync(bill.Id);
                    }
                    else
                    {
                        // Not yet due (more than 3 days): Send bill notification
                        await _notificationService.SendBillNotificationAsync(bill.Id);
                    }
                    
                    notificationsSent++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Bill {bill.BillNumber}: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = "Bulk notifications completed",
                totalBills = unpaidBills.Count,
                notificationsSent,
                errors
            });
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
