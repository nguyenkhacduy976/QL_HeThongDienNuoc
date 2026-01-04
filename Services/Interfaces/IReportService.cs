using QL_HethongDiennuoc.Models.DTOs;

namespace QL_HethongDiennuoc.Services.Interfaces;

public interface IReportService
{
    Task<ConsumptionReportDto> GetConsumptionReportAsync(DateTime? startDate, DateTime? endDate, int? customerId);
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime? startDate, DateTime? endDate);
    Task<List<OutstandingBillDto>> GetOutstandingBillsAsync();
}

public interface INotificationService
{
    Task SendBillNotificationAsync(int billId);
    Task SendPaymentDueReminderAsync(int billId);
    Task SendPaymentOverdueReminderAsync(int billId, int daysOverdue);
    Task SendEmailAsync(string to, string subject, string body);
    Task SendSmsAsync(string phoneNumber, string message);
}
