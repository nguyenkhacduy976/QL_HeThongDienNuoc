using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    private readonly IConfiguration _configuration;

    public NotificationService(
        ApplicationDbContext context, 
        ILogger<NotificationService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendBillNotificationAsync(int billId)
    {
        var bill = await _context.Bills
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == billId);

        if (bill == null) return;

        var subject = $"Hóa đơn điện nước số {bill.BillNumber}";
        var body = $@"
Kính gửi {bill.Customer.FullName},

Hóa đơn điện nước của quý khách đã được tạo:
- Số hóa đơn: {bill.BillNumber}
- Số tiền: {bill.Amount:#,##0} VNĐ
- Hạn thanh toán: {bill.DueDate:dd/MM/yyyy}

Vui lòng thanh toán đúng hạn để tránh bị cắt dịch vụ.

Trân trọng,
Ban quản lý điện nước
";

        await SendEmailAsync(bill.Customer.Email ?? "", subject, body);

        if (!string.IsNullOrEmpty(bill.Customer.PhoneNumber))
        {
            var smsMessage = $"Hoa don {bill.BillNumber}: {bill.Amount:#,0} VND. Han {bill.DueDate:dd/MM}";
            await SendSmsAsync(bill.Customer.PhoneNumber, smsMessage);
        }
    }

    public async Task SendPaymentDueReminderAsync(int billId)
    {
        var bill = await _context.Bills
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == billId);

        if (bill == null) return;

        var daysUntilDue = (bill.DueDate - DateTime.Now).Days;

        var subject = $"Nhắc nhở: Hóa đơn {bill.BillNumber} sắp đến hạn thanh toán";
        var body = $@"
Kính gửi {bill.Customer.FullName},

Hóa đơn điện nước của quý khách sắp đến hạn thanh toán:
- Số hóa đơn: {bill.BillNumber}
- Số tiền: {bill.Amount:#,##0} VNĐ
- Hạn thanh toán: {bill.DueDate:dd/MM/yyyy} (còn {daysUntilDue} ngày)

Vui lòng thanh toán trước hạn để tránh phát sinh phí chậm thanh toán.

Trân trọng,
Ban quản lý điện nước
";

        await SendEmailAsync(bill.Customer.Email ?? "", subject, body);

        if (!string.IsNullOrEmpty(bill.Customer.PhoneNumber))
        {
            var smsMessage = $"Nhac nho: Hoa don {bill.BillNumber} {bill.Amount:#,0} VND den han {bill.DueDate:dd/MM} (con {daysUntilDue} ngay).";
            await SendSmsAsync(bill.Customer.PhoneNumber, smsMessage);
        }

        // Update reminder tracking
        bill.LastReminderSent = DateTime.Now;
        bill.ReminderCount++;
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Payment due reminder sent for Bill #{BillId}", billId);
    }

    public async Task SendPaymentOverdueReminderAsync(int billId, int daysOverdue)
    {
        var bill = await _context.Bills
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == billId);

        if (bill == null) return;

        var urgencyLevel = daysOverdue <= 3 ? "KHẨN" : "RẤT KHẨN";
        var subject = $"[{urgencyLevel}] Hóa đơn {bill.BillNumber} đã quá hạn {daysOverdue} ngày";
        var body = $@"
Kính gửi {bill.Customer.FullName},

Hóa đơn điện nước của quý khách đã quá hạn thanh toán:
- Số hóa đơn: {bill.BillNumber}
- Số tiền: {bill.Amount:#,##0} VNĐ
- Hạn thanh toán: {bill.DueDate:dd/MM/yyyy}
- Đã quá hạn: {daysOverdue} ngày

⚠️ CẢNH BÁO: Nếu không thanh toán trong vòng 3 ngày, dịch vụ của quý khách sẽ bị tạm ngưng.

Vui lòng thanh toán ngay để tránh gián đoạn dịch vụ.
Liên hệ: admin@qldienuoc.vn

Trân trọng,
Ban quản lý điện nước
";

        await SendEmailAsync(bill.Customer.Email ?? "", subject, body);

        if (!string.IsNullOrEmpty(bill.Customer.PhoneNumber))
        {
            var smsMessage = $"[{urgencyLevel}] Hoa don {bill.BillNumber} qua han {daysOverdue} ngay. {bill.Amount:#,0} VND. Thanh toan gap!";
            await SendSmsAsync(bill.Customer.PhoneNumber, smsMessage);
        }

        // Update reminder tracking
        bill.LastReminderSent = DateTime.Now;
        bill.ReminderCount++;
        await _context.SaveChangesAsync();

        _logger.LogInformation("⚠️ Overdue payment reminder sent for Bill #{BillId}, Days Overdue: {DaysOverdue}", billId, daysOverdue);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            // Get email settings from configuration
            var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var senderEmail = _configuration["Email:SenderEmail"] ?? "noreply@qldienuoc.vn";
            var senderName = _configuration["Email:SenderName"] ?? "Hệ thống Điện Nước";
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];

            using var client = new MailKit.Net.Smtp.SmtpClient();
            
            // Connect to SMTP server
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            
            // Authenticate if credentials provided
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(username, password);
            }
            
            // Create message
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress(senderName, senderEmail));
            message.To.Add(new MimeKit.MailboxAddress("", to));
            message.Subject = subject;
            
            var bodyBuilder = new MimeKit.BodyBuilder
            {
                TextBody = body,
                HtmlBody = $"<pre>{body}</pre>" // Simple HTML formatting
            };
            message.Body = bodyBuilder.ToMessageBody();
            
            // Send message
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("✅ Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {To}", to);
            // Don't throw - notification failure shouldn't break the main flow
        }
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            var provider = _configuration["Sms:Provider"];
            
            if (provider == "ESMS")
            {
                // ESMS.vn implementation
                var apiKey = _configuration["Sms:ApiKey"];
                var secretKey = _configuration["Sms:SecretKey"];
                var brandName = _configuration["Sms:BrandName"];
                
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogWarning("⚠️ SMS provider configured but missing API credentials");
                    return;
                }
                
                _logger.LogInformation("📱 Attempting to send SMS to {PhoneNumber} via ESMS", phoneNumber);
                
                using var httpClient = new HttpClient();
                var requestData = new
                {
                    Phone = phoneNumber,
                    Content = message,
                    ApiKey = apiKey,
                    SecretKey = secretKey,
                    Brandname = brandName,
                    SmsType = 2 // 2 = CSKH
                };
                
                var response = await httpClient.PostAsJsonAsync(
                    "http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_post_json/",
                    requestData);
                
                var responseBody = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ SMS sent successfully to {PhoneNumber}. Response: {Response}", phoneNumber, responseBody);
                }
                else
                {
                    _logger.LogWarning("⚠️ SMS failed with status {Status}. Response: {Response}", response.StatusCode, responseBody);
                }
            }
            else
            {
                // Mock/Log for other providers
                _logger.LogInformation("📱 SMS would be sent to {PhoneNumber}", phoneNumber);
                _logger.LogInformation("Message: {Message}", message);
            }
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send SMS to {PhoneNumber}", phoneNumber);
        }
    }
}
