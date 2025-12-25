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
            .OrderByDescending(r => r.ReadingDate)
            .ToListAsync();
        return _mapper.Map<List<ReadingDto>>(readings);
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
}
