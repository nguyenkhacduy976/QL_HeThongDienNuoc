using QL_HethongDiennuoc.Models.DTOs;
using QL_HethongDiennuoc.Models.Entities;

namespace QL_HethongDiennuoc.Services.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
    Task<bool> DeleteCustomerAsync(int id);
}

public interface IMeterService
{
    Task<List<MeterDto>> GetAllMetersAsync();
    Task<MeterDto?> GetMeterByIdAsync(int id);
    Task<List<MeterDto>> GetMetersByCustomerIdAsync(int customerId);
    Task<MeterDto> CreateMeterAsync(CreateMeterDto dto);
    Task<MeterDto?> UpdateMeterAsync(int id, UpdateMeterDto dto);
    Task<bool> DeleteMeterAsync(int id);
}

public interface IReadingService
{
    Task<List<ReadingDto>> GetAllReadingsAsync();
    Task<ReadingDto?> GetReadingByIdAsync(int id);
    Task<List<ReadingDto>> GetReadingsByMeterIdAsync(int meterId);
    Task<ReadingDto> CreateReadingAsync(CreateReadingDto dto);
}

public interface IBillingService
{
    Task<List<BillDto>> GetAllBillsAsync();
    Task<BillDto?> GetBillByIdAsync(int id);
    Task<List<BillDto>> GetBillsByCustomerIdAsync(int customerId);
    Task<BillDto> GenerateBillAsync(GenerateBillDto dto);
    Task<BillDto?> UpdateBillStatusAsync(int id, BillStatus status);
    Task<byte[]> GenerateBillPdfAsync(int billId);
}

public interface IPaymentService
{
    Task<List<PaymentDto>> GetAllPaymentsAsync();
    Task<PaymentDto?> GetPaymentByIdAsync(int id);
    Task<List<PaymentDto>> GetPaymentsByBillIdAsync(int billId);
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto);
}

public interface IServiceManagementService
{
    Task<List<ServiceStatusDto>> GetServicesByCustomerIdAsync(int customerId);
    Task<ServiceStatusDto> SuspendServiceAsync(SuspendServiceDto dto);
    Task<ServiceStatusDto> RestoreServiceAsync(RestoreServiceDto dto);
}

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    Task<User> RegisterAsync(RegisterDto dto);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
}
