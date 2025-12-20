using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public PaymentService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PaymentDto>> GetAllPaymentsAsync()
    {
        var payments = await _context.Payments
            .Include(p => p.Bill)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
        return _mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Bill)
            .FirstOrDefaultAsync(p => p.Id == id);
        return payment == null ? null : _mapper.Map<PaymentDto>(payment);
    }

    public async Task<List<PaymentDto>> GetPaymentsByBillIdAsync(int billId)
    {
        var payments = await _context.Payments
            .Include(p => p.Bill)
            .Where(p => p.BillId == billId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
        return _mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var bill = await _context.Bills.FindAsync(dto.BillId);
        if (bill == null)
            throw new Exception("Bill not found");

        // Create payment
        var payment = _mapper.Map<Payment>(dto);
        payment.PaymentDate = DateTime.Now;

        _context.Payments.Add(payment);

        // Update bill
        bill.PaidAmount += dto.Amount;
        
        if (bill.PaidAmount >= bill.Amount)
        {
            bill.Status = BillStatus.Paid;
        }
        else if (bill.PaidAmount > 0)
        {
            bill.Status = BillStatus.Partial;
        }

        await _context.SaveChangesAsync();

        // Reload with bill data
        await _context.Entry(payment).Reference(p => p.Bill).LoadAsync();

        return _mapper.Map<PaymentDto>(payment);
    }
}
