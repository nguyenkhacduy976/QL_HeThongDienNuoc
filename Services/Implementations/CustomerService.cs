using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CustomerService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _context.Customers.ToListAsync();
        return _mapper.Map<List<CustomerDto>>(customers);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        return customer == null ? null : _mapper.Map<CustomerDto>(customer);
    }



    public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return null;

        _mapper.Map(dto, customer);
        await _context.SaveChangesAsync();

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Meters)
                .ThenInclude(m => m.Readings)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (customer == null) return false;

        // Kiểm tra xem có công tơ với chỉ số không
        var hasReadings = customer.Meters.Any(m => m.Readings.Any());
        if (hasReadings)
        {
            throw new Exception("Không thể xóa khách hàng đã có lịch sử ghi chỉ số! Hãy vô hiệu hóa thay vì xóa.");
        }

        // Kiểm tra xem có hóa đơn không
        var hasBills = await _context.Bills.AnyAsync(b => b.CustomerId == id);
        if (hasBills)
        {
            throw new Exception("Không thể xóa khách hàng đã có hóa đơn! Hãy vô hiệu hóa thay vì xóa.");
        }

        // Xóa các công tơ (nếu chưa có readings)
        if (customer.Meters.Any())
        {
            _context.Meters.RemoveRange(customer.Meters);
        }

        // Also delete the associated User if exists
        if (customer.UserId.HasValue)
        {
            var user = await _context.Users.FindAsync(customer.UserId.Value);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return true;
    }
}
