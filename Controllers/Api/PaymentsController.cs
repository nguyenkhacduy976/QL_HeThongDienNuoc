using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer,Cookies")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;
    private readonly ApplicationDbContext _context;

    public PaymentsController(
        IPaymentService paymentService,
        INotificationService notificationService,
        ApplicationDbContext context)
    {
        _paymentService = paymentService;
        _notificationService = notificationService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _paymentService.GetAllPaymentsAsync();
        return Ok(payments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
            return NotFound(new { message = "Payment not found" });

        return Ok(payment);
    }

    [HttpGet("bill/{billId}")]
    public async Task<IActionResult> GetByBillId(int billId)
    {
        var payments = await _paymentService.GetPaymentsByBillIdAsync(billId);
        return Ok(payments);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        try
        {
            var payment = await _paymentService.CreatePaymentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{billId}/send-reminder")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> SendReminder(int billId)
    {
        try
        {
            var bill = await _context.Bills
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == billId);

            if (bill == null)
                return NotFound(new { message = "Bill not found" });

            if (bill.Status == BillStatus.Paid)
                return BadRequest(new { message = "Bill is already paid" });

            // Calculate days overdue (can be negative if not yet due)
            var daysOverdue = (DateTime.Now - bill.DueDate).Days;
            
            // Send overdue reminder
            await _notificationService.SendPaymentOverdueReminderAsync(billId, daysOverdue);

            return Ok(new { message = "Payment reminder sent successfully", billId, billNumber = bill.BillNumber });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("send-bulk-reminders")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendBulkReminders()
    {
        try
        {
            var unpaidBills = await _context.Bills
                .Where(b => b.Status == BillStatus.Pending || b.Status == BillStatus.Overdue)
                .ToListAsync();

            int remindersSent = 0;
            var errors = new List<string>();

            foreach (var bill in unpaidBills)
            {
                try
                {
                    // Calculate days overdue (can be negative if not yet due)
                    var daysOverdue = (DateTime.Now - bill.DueDate).Days;
                    
                    // Send overdue reminder
                    await _notificationService.SendPaymentOverdueReminderAsync(bill.Id, daysOverdue);
                    remindersSent++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Bill {bill.BillNumber}: {ex.Message}");
                }
            }

            return Ok(new 
            { 
                message = "Bulk reminders completed", 
                totalBills = unpaidBills.Count,
                remindersSent,
                errors
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("reminder-stats")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetReminderStats()
    {
        try
        {
            var today = DateTime.Now.Date;

            var stats = new
            {
                totalUnpaidBills = await _context.Bills
                    .CountAsync(b => b.Status == BillStatus.Pending || b.Status == BillStatus.Overdue),
                    
                billsDueSoon = await _context.Bills
                    .CountAsync(b => b.Status == BillStatus.Pending && 
                                    b.DueDate.Date > today && 
                                    b.DueDate.Date <= today.AddDays(3)),
                    
                overdueBills = await _context.Bills
                    .CountAsync(b => b.Status == BillStatus.Overdue),
                    
                remindersSentToday = await _context.Bills
                    .CountAsync(b => b.LastReminderSent != null && 
                                    b.LastReminderSent.Value.Date == today),
                    
                billsNeedingReminder = await _context.Bills
                    .CountAsync(b => (b.Status == BillStatus.Pending || b.Status == BillStatus.Overdue) &&
                                    (b.LastReminderSent == null || b.LastReminderSent.Value.Date < today))
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
