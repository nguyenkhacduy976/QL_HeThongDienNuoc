using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
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

    public async Task SendPaymentConfirmationAsync(int paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Bill)
                .ThenInclude(b => b.Customer)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return;

        var subject = $"Xác nhận thanh toán hóa đơn {payment.Bill.BillNumber}";
        var body = $@"
Kính gửi {payment.Bill.Customer.FullName},

Chúng tôi đã nhận được khoản thanh toán của quý khách:
- Hóa đơn: {payment.Bill.BillNumber}
- Số tiền: {payment.Amount:#,##0} VNĐ
- Ngày thanh toán: {payment.PaymentDate:dd/MM/yyyy HH:mm}
- Phương thức: {payment.Method}

Cảm ơn quý khách đã thanh toán.

Trân trọng,
Ban quản lý điện nước
";

        await SendEmailAsync(payment.Bill.Customer.Email ?? "", subject, body);
    }

    public async Task SendServiceSuspensionWarningAsync(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null) return;

        var subject = "Cảnh báo: Sắp cắt dịch vụ do nợ tiền";
        var body = $@"
Kính gửi {customer.FullName},

Tài khoản của quý khách có hóa đơn quá hạn chưa thanh toán.
Vui lòng thanh toán trong vòng 3 ngày để tránh bị cắt dịch vụ.

Liên hệ: admin@qldienuoc.vn

Trân trọng,
Ban quản lý điện nước
";

        await SendEmailAsync(customer.Email ?? "", subject, body);

        if (!string.IsNullOrEmpty(customer.PhoneNumber))
        {
            await SendSmsAsync(customer.PhoneNumber, "Canh bao: Tai khoan co hoa don qua han. Thanh toan de tranh cat dich vu!");
        }
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
                    _logger.LogWarning("SMS provider configured but missing API credentials");
                    return;
                }
                
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
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ SMS sent successfully to {PhoneNumber}", phoneNumber);
                }
                else
                {
                    _logger.LogWarning("⚠️ SMS failed with status {Status}", response.StatusCode);
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
