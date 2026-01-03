using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Services.Implementations;

public class ReadingService : IReadingService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ReadingService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ReadingDto>> GetAllReadingsAsync()
    {
        var readings = await _context.Readings
            .Include(r => r.Meter)
                .ThenInclude(m => m.Customer)
            .OrderByDescending(r => r.ReadingDate)
            .ToListAsync();
        
        return readings.Select(r => new ReadingDto
        {
            Id = r.Id,
            ReadingDate = r.ReadingDate,
            PreviousReading = r.PreviousReading,
            CurrentReading = r.CurrentReading,
            Consumption = r.Consumption,
            Notes = r.Notes,
            MeterId = r.MeterId,
            MeterNumber = r.Meter?.MeterNumber ?? string.Empty,
            CustomerName = r.Meter?.Customer?.FullName ?? string.Empty,
            MeterType = r.Meter?.Type.ToString() ?? string.Empty
        }).ToList();
    }

    public async Task<ReadingDto?> GetReadingByIdAsync(int id)
    {
        var reading = await _context.Readings
            .Include(r => r.Meter)
            .FirstOrDefaultAsync(r => r.Id == id);
        return reading == null ? null : _mapper.Map<ReadingDto>(reading);
    }

    public async Task<List<ReadingDto>> GetReadingsByMeterIdAsync(int meterId)
    {
        var readings = await _context.Readings
            .Include(r => r.Meter)
            .Where(r => r.MeterId == meterId)
            .OrderByDescending(r => r.ReadingDate)
            .ToListAsync();
        return _mapper.Map<List<ReadingDto>>(readings);
    }

    public async Task<ReadingDto> CreateReadingAsync(CreateReadingDto dto)
    {
        // Get the meter
        var meter = await _context.Meters.FindAsync(dto.MeterId);
        if (meter == null)
            throw new Exception("Meter not found");

        // Get previous reading
        var previousReading = await _context.Readings
            .Where(r => r.MeterId == dto.MeterId)
            .OrderByDescending(r => r.ReadingDate)
            .FirstOrDefaultAsync();

        // Use previous reading if exists, otherwise use meter's initial reading
        decimal previousValue = previousReading?.CurrentReading ?? meter.InitialReading;

        // Validate
        if (dto.CurrentReading < previousValue)
            throw new Exception($"Chỉ số mới ({dto.CurrentReading}) không được nhỏ hơn chỉ số cũ ({previousValue})");

        // Create reading
        var reading = new Reading
        {
            MeterId = dto.MeterId,
            ReadingDate = dto.ReadingDate,
            PreviousReading = previousValue,
            CurrentReading = dto.CurrentReading,
            Consumption = dto.CurrentReading - previousValue,
            Notes = dto.Notes
        };

        _context.Readings.Add(reading);
        await _context.SaveChangesAsync();

        // Reload with meter data
        await _context.Entry(reading).Reference(r => r.Meter).LoadAsync();

        return _mapper.Map<ReadingDto>(reading);
    }

    public async Task<ReadingDto?> UpdateReadingAsync(int id, UpdateReadingDto dto)
    {
        var reading = await _context.Readings
            .Include(r => r.Bill)
            .Include(r => r.Meter)
                .ThenInclude(m => m.Customer)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (reading == null)
            return null;

        // Kiểm tra xem đã có hóa đơn liên quan chưa
        if (reading.Bill != null)
            throw new Exception("Không thể sửa chỉ số đã tạo hóa đơn!");

        // Validate chỉ số mới phải >= chỉ số cũ
        if (dto.CurrentReading < reading.PreviousReading)
            throw new Exception($"Chỉ số mới ({dto.CurrentReading}) không được nhỏ hơn chỉ số cũ ({reading.PreviousReading})");

        reading.ReadingDate = dto.ReadingDate;
        reading.CurrentReading = dto.CurrentReading;
        reading.Consumption = dto.CurrentReading - reading.PreviousReading;
        reading.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return new ReadingDto
        {
            Id = reading.Id,
            ReadingDate = reading.ReadingDate,
            PreviousReading = reading.PreviousReading,
            CurrentReading = reading.CurrentReading,
            Consumption = reading.Consumption,
            Notes = reading.Notes,
            MeterId = reading.MeterId,
            MeterNumber = reading.Meter?.MeterNumber ?? string.Empty,
            CustomerName = reading.Meter?.Customer?.FullName ?? string.Empty,
            MeterType = reading.Meter?.Type.ToString() ?? string.Empty
        };
    }

    public async Task<bool> DeleteReadingAsync(int id)
    {
        var reading = await _context.Readings
            .Include(r => r.Bill)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (reading == null)
            return false;

        // Kiểm tra xem đã có hóa đơn liên quan chưa
        if (reading.Bill != null)
            throw new Exception("Không thể xóa chỉ số đã tạo hóa đơn!");

        _context.Readings.Remove(reading);
        await _context.SaveChangesAsync();
        return true;
    }
}
