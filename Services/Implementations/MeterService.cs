using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Services.Implementations;

public class MeterService : IMeterService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public MeterService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MeterDto>> GetAllMetersAsync()
    {
        var meters = await _context.Meters
            .Include(m => m.Customer)
            .ToListAsync();
        return _mapper.Map<List<MeterDto>>(meters);
    }

    public async Task<MeterDto?> GetMeterByIdAsync(int id)
    {
        var meter = await _context.Meters
            .Include(m => m.Customer)
            .FirstOrDefaultAsync(m => m.Id == id);
        return meter == null ? null : _mapper.Map<MeterDto>(meter);
    }

    public async Task<List<MeterDto>> GetMetersByCustomerIdAsync(int customerId)
    {
        var meters = await _context.Meters
            .Include(m => m.Customer)
            .Where(m => m.CustomerId == customerId)
            .ToListAsync();
        return _mapper.Map<List<MeterDto>>(meters);
    }

    public async Task<MeterDto> CreateMeterAsync(CreateMeterDto dto)
    {
        var meter = _mapper.Map<Meter>(dto);
        meter.IsActive = true;

        _context.Meters.Add(meter);
        await _context.SaveChangesAsync();

        // Reload with customer data
        await _context.Entry(meter).Reference(m => m.Customer).LoadAsync();

        return _mapper.Map<MeterDto>(meter);
    }

    public async Task<MeterDto?> UpdateMeterAsync(int id, UpdateMeterDto dto)
    {
        var meter = await _context.Meters
            .Include(m => m.Customer)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (meter == null) return null;

        if (dto.MeterNumber != null) meter.MeterNumber = dto.MeterNumber;
        if (dto.Location != null) meter.Location = dto.Location;
        if (dto.IsActive.HasValue) meter.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return _mapper.Map<MeterDto>(meter);
    }

    public async Task<bool> DeleteMeterAsync(int id)
    {
        var meter = await _context.Meters.FindAsync(id);
        if (meter == null) return false;

        _context.Meters.Remove(meter);
        await _context.SaveChangesAsync();

        return true;
    }
}
