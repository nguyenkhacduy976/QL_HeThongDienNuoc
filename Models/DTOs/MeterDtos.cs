namespace QL_HethongDiennuoc.Models.DTOs;

public class MeterDto
{
    public int Id { get; set; }
    public string MeterNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime InstallDate { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}

public class CreateMeterDto
{
    public string MeterNumber { get; set; } = string.Empty;
    public int Type { get; set; } // 1=Electric, 2=Water
    public DateTime? InstallDate { get; set; }
    public string? Location { get; set; }
    public int CustomerId { get; set; }
}

public class UpdateMeterDto
{
    public string? MeterNumber { get; set; }
    public string? Location { get; set; }
    public bool? IsActive { get; set; }
}
