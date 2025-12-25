using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Services.Implementations;

public class DebtManagementService : IDebtManagementService
{
    private readonly ApplicationDbContext _context;

    public DebtManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DebtManagementDto>> GetCustomersWithDebtAsync(bool? isServiceActive = null)
    {
        // Get all customers with unpaid bills (AsNoTracking for fresh data)
        var customersQuery = _context.Customers
            .AsNoTracking()
            .Include(c => c.Bills)
            .Where(c => c.Bills.Any(b => b.Status != BillStatus.Paid));

        // Filter by service status if specified
        if (isServiceActive.HasValue)
        {
            customersQuery = customersQuery.Where(c => c.IsActive == isServiceActive.Value);
        }

        var customers = await customersQuery.ToListAsync();

        var result = new List<DebtManagementDto>();

        foreach (var customer in customers)
        {
            var unpaidBills = customer.Bills.Where(b => b.Status != BillStatus.Paid).ToList();
            
            if (unpaidBills.Any())
            {
                var totalDebt = unpaidBills.Sum(b => b.Amount - b.PaidAmount);
                var oldestBill = unpaidBills.OrderBy(b => b.IssueDate).FirstOrDefault();

                result.Add(new DebtManagementDto
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.FullName,
                    Phone = customer.PhoneNumber ?? "N/A",
                    Address = customer.Address,
                    TotalDebt = totalDebt,
                    UnpaidBillsCount = unpaidBills.Count,
                    OldestUnpaidBillDate = oldestBill?.IssueDate,
                    IsServiceActive = customer.IsActive,
                    ServiceSuspendedDate = !customer.IsActive ? customer.CreatedDate : null
                });
            }
        }

        return result.OrderByDescending(d => d.TotalDebt).ToList();
    }

    public async Task<bool> SuspendServiceAsync(int customerId, string? notes = null)
    {
        // Reload customer from DB to get fresh data
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);
            
        if (customer == null)
            throw new Exception("Khách hàng không tồn tại");
            
        if (!customer.IsActive)
            throw new Exception("Dịch vụ đã bị cắt trước đó");

        customer.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreServiceAsync(int customerId, string? notes = null)
    {
        // Reload customer from DB to get fresh data
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);
            
        if (customer == null)
            throw new Exception("Khách hàng không tồn tại");
            
        if (customer.IsActive)
            throw new Exception("Dịch vụ đang hoạt động, không cần khôi phục");

        customer.IsActive = true;
        await _context.SaveChangesAsync();

        return true;
    }
}
