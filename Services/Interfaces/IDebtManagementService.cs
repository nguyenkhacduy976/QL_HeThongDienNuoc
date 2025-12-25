using QL_HethongDiennuoc.Models.DTOs;

namespace QL_HethongDiennuoc.Services.Interfaces;

public interface IDebtManagementService
{
    Task<List<DebtManagementDto>> GetCustomersWithDebtAsync(bool? isServiceActive = null);
    Task<bool> SuspendServiceAsync(int customerId, string? notes = null);
    Task<bool> RestoreServiceAsync(int customerId, string? notes = null);
}
