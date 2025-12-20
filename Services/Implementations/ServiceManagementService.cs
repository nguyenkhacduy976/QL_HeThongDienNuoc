using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Data;
using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;
using QL_HethongDiennuoc.Services.Interfaces;

namespace QL_HethongDiennuoc.Services.Implementations;

public class ServiceManagementService : IServiceManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ServiceManagementService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ServiceStatusDto>> GetServicesByCustomerIdAsync(int customerId)
    {
        var services = await _context.Services
            .Include(s => s.Customer)
            .Where(s => s.CustomerId == customerId)
            .ToListAsync();
        return _mapper.Map<List<ServiceStatusDto>>(services);
    }

    public async Task<ServiceStatusDto> SuspendServiceAsync(SuspendServiceDto dto)
    {
        // Check if service record exists
        var service = await _context.Services
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.CustomerId == dto.CustomerId && s.Type == (ServiceType)dto.ServiceType);

        if (service == null)
        {
            // Create new service record
            service = new Service
            {
                CustomerId = dto.CustomerId,
                Type = (ServiceType)dto.ServiceType,
                Status = ServiceStatus.Suspended,
                SuspendDate = DateTime.Now,
                Reason = dto.Reason
            };
            _context.Services.Add(service);
        }
        else
        {
            // Update existing
            service.Status = ServiceStatus.Suspended;
            service.SuspendDate = DateTime.Now;
            service.Reason = dto.Reason;
        }

        await _context.SaveChangesAsync();

        // Reload customer data
        await _context.Entry(service).Reference(s => s.Customer).LoadAsync();

        return _mapper.Map<ServiceStatusDto>(service);
    }

    public async Task<ServiceStatusDto> RestoreServiceAsync(RestoreServiceDto dto)
    {
        var service = await _context.Services
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == dto.ServiceId);

        if (service == null)
            throw new Exception("Service not found");

        service.Status = ServiceStatus.Active;
        service.RestoreDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return _mapper.Map<ServiceStatusDto>(service);
    }
}
