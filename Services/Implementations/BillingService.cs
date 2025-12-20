using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Helpers;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Services.Implementations;

public class BillingService : IBillingService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public BillingService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<BillDto>> GetAllBillsAsync()
    {
        var bills = await _context.Bills
            .Include(b => b.Customer)
            .Include(b => b.Reading)
                .ThenInclude(r => r.Meter)
            .OrderByDescending(b => b.IssueDate)
            .ToListAsync();
        return _mapper.Map<List<BillDto>>(bills);
    }

    public async Task<BillDto?> GetBillByIdAsync(int id)
    {
        var bill = await _context.Bills
            .Include(b => b.Customer)
            .Include(b => b.Reading)
                .ThenInclude(r => r.Meter)
            .FirstOrDefaultAsync(b => b.Id == id);
        return bill == null ? null : _mapper.Map<BillDto>(bill);
    }

    public async Task<List<BillDto>> GetBillsByCustomerIdAsync(int customerId)
    {
        var bills = await _context.Bills
            .Include(b => b.Customer)
            .Include(b => b.Reading)
                .ThenInclude(r => r.Meter)
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.IssueDate)
            .ToListAsync();
        return _mapper.Map<List<BillDto>>(bills);
    }

    public async Task<BillDto> GenerateBillAsync(GenerateBillDto dto)
    {
        // Get reading with meter info
        var reading = await _context.Readings
            .Include(r => r.Meter)
                .ThenInclude(m => m.Customer)
            .FirstOrDefaultAsync(r => r.Id == dto.ReadingId);

        if (reading == null)
            throw new Exception("Reading not found");

        // Check if bill already exists
        var existingBill = await _context.Bills
            .FirstOrDefaultAsync(b => b.ReadingId == dto.ReadingId);
        if (existingBill != null)
            throw new Exception("Bill already exists for this reading");

        // Get tariffs
        var tariffs = await _context.Tariffs
            .Where(t => t.ServiceType == reading.Meter.Type && t.IsActive)
            .ToListAsync();

        // Calculate amount using tiered pricing
        decimal amount = TariffCalculator.CalculateBill(reading.Consumption, reading.Meter.Type, tariffs);

        // Generate bill number
        string billNumber = $"HD{DateTime.Now:yyyyMMdd}{reading.Meter.Type.ToString().Substring(0, 1)}{reading.Id:D6}";

        // Create bill
        var bill = new Bill
        {
            BillNumber = billNumber,
            Amount = amount,
            IssueDate = DateTime.Now,
            DueDate = dto.DueDate ?? DateTime.Now.AddDays(30),
            Status = BillStatus.Pending,
            PaidAmount = 0,
            ReadingId = reading.Id,
            CustomerId = reading.Meter.CustomerId
        };

        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        // Reload with all relationships
        await _context.Entry(bill).Reference(b => b.Customer).LoadAsync();
        await _context.Entry(bill).Reference(b => b.Reading).LoadAsync();
        await _context.Entry(bill.Reading).Reference(r => r.Meter).LoadAsync();

        return _mapper.Map<BillDto>(bill);
    }

    public async Task<BillDto?> UpdateBillStatusAsync(int id, BillStatus status)
    {
        var bill = await _context.Bills
            .Include(b => b.Customer)
            .Include(b => b.Reading)
                .ThenInclude(r => r.Meter)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bill == null) return null;

        bill.Status = status;
        await _context.SaveChangesAsync();

        return _mapper.Map<BillDto>(bill);
    }

    public async Task<byte[]> GenerateBillPdfAsync(int billId)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var bill = await _context.Bills
            .Include(b => b.Customer)
            .Include(b => b.Reading)
                .ThenInclude(r => r.Meter)
            .FirstOrDefaultAsync(b => b.Id == billId);

        if (bill == null)
            throw new Exception("Bill not found");

        // Get tier breakdown
        var tariffs = await _context.Tariffs
            .Where(t => t.ServiceType == bill.Reading.Meter.Type && t.IsActive)
            .ToListAsync();
        var breakdown = TariffCalculator.GetTierBreakdown(bill.Reading.Consumption, bill.Reading.Meter.Type, tariffs);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text("HÓA ĐƠN ĐIỆN NƯỚC")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(10);

                        // Bill info
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Số hóa đơn: {bill.BillNumber}").Bold();
                            row.RelativeItem().AlignRight().Text($"Ngày phát hành: {bill.IssueDate:dd/MM/yyyy}");
                        });

                        col.Item().LineHorizontal(1);

                        // Customer info
                        col.Item().Text("THÔNG TIN KHÁCH HÀNG").FontSize(14).Bold();
                        col.Item().Text($"Họ tên: {bill.Customer.FullName}");
                        col.Item().Text($"Địa chỉ: {bill.Customer.Address}");
                        col.Item().Text($"Số điện thoại: {bill.Customer.PhoneNumber}");

                        col.Item().PaddingTop(10).LineHorizontal(1);

                        // Meter reading info
                        col.Item().Text("CHI TIẾT TIÊU THỤ").FontSize(14).Bold();
                        col.Item().Text($"Loại dịch vụ: {(bill.Reading.Meter.Type == MeterType.Electric ? "Điện" : "Nước")}");
                        col.Item().Text($"Số công tơ: {bill.Reading.Meter.MeterNumber}");
                        col.Item().Text($"Kỳ đọc: {bill.Reading.ReadingDate:dd/MM/yyyy}");
                        col.Item().Text($"Chỉ số cũ: {bill.Reading.PreviousReading:#,##0.##}");
                        col.Item().Text($"Chỉ số mới: {bill.Reading.CurrentReading:#,##0.##}");
                        col.Item().Text($"Tiêu thụ: {bill.Reading.Consumption:#,##0.##} {(bill.Reading.Meter.Type == MeterType.Electric ? "kWh" : "m³")}").Bold();

                        col.Item().PaddingTop(10).LineHorizontal(1);

                        // Tier breakdown
                        col.Item().Text("BẢNG TÍNH GIÁ BẬC THANG").FontSize(14).Bold();
                        
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Bậc").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Mô tả").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Tiêu thụ").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Thành tiền (VNĐ)").Bold();
                            });

                            foreach (var tier in breakdown)
                            {
                                if (tier.Consumption > 0)
                                {
                                    table.Cell().Border(1).Padding(5).Text(tier.Tier.ToString());
                                    table.Cell().Border(1).Padding(5).Text(tier.Description);
                                    table.Cell().Border(1).Padding(5).AlignRight().Text($"{tier.Consumption:#,##0.##}");
                                    table.Cell().Border(1).Padding(5).AlignRight().Text($"{tier.Amount:#,##0}");
                                }
                            }
                        });

                        col.Item().PaddingTop(10).LineHorizontal(1);

                        // Total
                        col.Item().AlignRight().Text($"TỔNG CỘNG: {bill.Amount:#,##0} VNĐ").FontSize(16).Bold().FontColor(Colors.Red.Darken1);
                        col.Item().AlignRight().Text($"Hạn thanh toán: {bill.DueDate:dd/MM/yyyy}").FontColor(Colors.Red.Medium);

                        col.Item().PaddingTop(20).LineHorizontal(1);
                        col.Item().Text("Cảm ơn quý khách đã sử dụng dịch vụ!").FontSize(10).Italic();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
