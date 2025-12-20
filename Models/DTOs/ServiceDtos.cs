namespace QL_HethongDiennuoc.Models.DTOs;

public class ServiceStatusDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SuspendDate { get; set; }
    public DateTime? RestoreDate { get; set; }
    public string? Reason { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}

public class SuspendServiceDto
{
    public int CustomerId { get; set; }
    public int ServiceType { get; set; } // 1=Electric, 2=Water
    public string? Reason { get; set; }
}

public class RestoreServiceDto
{
    public int ServiceId { get; set; }
}
