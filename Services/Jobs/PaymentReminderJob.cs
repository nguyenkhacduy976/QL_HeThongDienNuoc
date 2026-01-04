using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Services.Jobs;

public class PaymentReminderJob : IHostedService, IDisposable
{
    private readonly ILogger<PaymentReminderJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private Timer? _timer;

    public PaymentReminderJob(
        ILogger<PaymentReminderJob> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("💼 Payment Reminder Job started");

        // Get scheduled time from configuration (default: 09:00)
        var scheduledTime = _configuration["PaymentReminders:ScheduledTime"] ?? "09:00";
        var now = DateTime.Now;
        var scheduledDateTime = DateTime.Parse($"{now:yyyy-MM-dd} {scheduledTime}");

        // If scheduled time has passed today, schedule for tomorrow
        if (scheduledDateTime < now)
        {
            scheduledDateTime = scheduledDateTime.AddDays(1);
        }

        var initialDelay = scheduledDateTime - now;
        
        _logger.LogInformation("⏰ Next reminder check scheduled at {ScheduledTime}", scheduledDateTime);

        _timer = new Timer(
            DoWork,
            null,
            initialDelay,
            TimeSpan.FromDays(1)); // Run daily

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            _logger.LogInformation("🔄 Starting payment reminder check...");

            var enabled = bool.Parse(_configuration["PaymentReminders:Enabled"] ?? "true");
            if (!enabled)
            {
                _logger.LogInformation("⏸️ Payment reminders are disabled in configuration");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var daysBeforeDue = int.Parse(_configuration["PaymentReminders:DaysBeforeDueDate"] ?? "3");
            var overdueReminderDays = _configuration.GetSection("PaymentReminders:OverdueReminderDays")
                .Get<int[]>() ?? new[] { 1, 3, 7 };
            var maxReminders = int.Parse(_configuration["PaymentReminders:MaxRemindersPerBill"] ?? "5");

            var today = DateTime.Now.Date;

            // Get bills needing reminders
            var billsNeedingReminders = await context.Bills
                .Include(b => b.Customer)
                .Where(b => b.Status == BillStatus.Pending || b.Status == BillStatus.Overdue)
                .Where(b => b.ReminderCount < maxReminders)
                .ToListAsync();

            int remindersSent = 0;

            foreach (var bill in billsNeedingReminders)
            {
                try
                {
                    var daysDiff = (bill.DueDate.Date - today).Days;

                    // Check if we should send a reminder
                    bool shouldSend = false;
                    string reminderType = "";

                    if (daysDiff == daysBeforeDue)
                    {
                        // Bill is 3 days (or configured) before due date
                        shouldSend = true;
                        reminderType = "DueSoon";
                        
                        await notificationService.SendPaymentDueReminderAsync(bill.Id);
                        remindersSent++;
                    }
                    else if (daysDiff < 0)
                    {
                        // Bill is overdue
                        var daysOverdue = Math.Abs(daysDiff);
                        
                        if (overdueReminderDays.Contains(daysOverdue))
                        {
                            // Check if we haven't sent a reminder today
                            if (bill.LastReminderSent == null || 
                                bill.LastReminderSent.Value.Date < today)
                            {
                                shouldSend = true;
                                reminderType = $"Overdue{daysOverdue}Days";
                                
                                await notificationService.SendPaymentOverdueReminderAsync(bill.Id, daysOverdue);
                                remindersSent++;
                            }
                        }
                    }

                    if (shouldSend)
                    {
                        _logger.LogInformation("📧 Sent {ReminderType} reminder for Bill #{BillId} ({BillNumber})", 
                            reminderType, bill.Id, bill.BillNumber);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to send reminder for Bill #{BillId}", bill.Id);
                }
            }

            _logger.LogInformation("✅ Payment reminder check completed. Sent {Count} reminders", remindersSent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in payment reminder job");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Payment Reminder Job stopped");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
