using QL_HethongDiennuoc.Models.Entities;

namespace QL_HethongDiennuoc.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Check if already seeded
        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        Console.WriteLine("⏳ Đang seed dữ liệu mẫu...");

        // Seed Admin User
        var admin = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Email = "admin@qldienuoc.vn",
            FullName = "Administrator",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedDate = DateTime.Now
        };
        context.Users.Add(admin);

        // Seed Staff User
        var staff = new User
        {
            Username = "staff01",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
            Email = "staff01@qldienuoc.vn",
            FullName = "Nhân viên 01",
            Role = UserRole.Staff,
            IsActive = true,
            CreatedDate = DateTime.Now
        };
        context.Users.Add(staff);

        // Seed Customer User
        var customerUser = new User
        {
            Username = "customer01",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Email = "customer01@qldienuoc.vn",
            FullName = "Khách hàng 01",
            Role = UserRole.Customer,
            IsActive = true,
            CreatedDate = DateTime.Now
        };
        context.Users.Add(customerUser);

        // Seed Electric Tariffs (Bảng giá điện bậc thang VN)
        var electricTariffs = new[]
        {
            new Tariff { ServiceType = MeterType.Electric, Tier = 1, MinKwh = 0, MaxKwh = 50, PricePerUnit = 1678, EffectiveDate = DateTime.Now, IsActive = true, Description = "Bậc 1: 0-50 kWh" },
            new Tariff { ServiceType = MeterType.Electric, Tier = 2, MinKwh = 51, MaxKwh = 100, PricePerUnit = 1734, EffectiveDate = DateTime.Now, IsActive = true, Description = "Bậc 2: 51-100 kWh" },
            new Tariff { ServiceType = MeterType.Electric, Tier = 3, MinKwh = 101, MaxKwh = 200, PricePerUnit = 2014, EffectiveDate = DateTime.Now, IsActive = true, Description = "Bậc 3: 101-200 kWh" },
            new Tariff { ServiceType = MeterType.Electric, Tier = 4, MinKwh = 201, MaxKwh = 300, PricePerUnit = 2536, EffectiveDate = DateTime.Now, IsActive = true, Description = "Bậc 4: 201-300 kWh" },
            new Tariff { ServiceType = MeterType.Electric, Tier = 5, MinKwh = 301, MaxKwh = 400, PricePerUnit = 2834, EffectiveDate = DateTime.Now, IsActive = true, Description = "Bậc 5: 301-400 kWh" },
            new Tariff { ServiceType = MeterType.Electric, Tier = 6, MinKwh = 401, MaxKwh = null, PricePerUnit = 2927, EffectiveDate = DateTime.Now, IsActive = true, Description = "Bậc 6: Trên 400 kWh" }
        };
        context.Tariffs.AddRange(electricTariffs);

        // Seed Water Tariffs (Bảng giá nước bậc thang VN)
        var waterTariffs = new[]
        {
            new Tariff { ServiceType = MeterType.Water, Tier = 1, MinKwh = 0, MaxKwh = 10, PricePerUnit = 6869, EffectiveDate = DateTime.Now, IsActive = true, Description = "Bậc 1: 0-10 m³" },
            new Tariff { ServiceType = MeterType.Water, Tier = 2, MinKwh = 11, MaxKwh = 20, PricePerUnit = 8110, EffectiveDate = DateTime.Now, IsActive = true, Description = "Bậc 2: 11-20 m³" },
            new Tariff { ServiceType = MeterType.Water, Tier = 3, MinKwh = 21, MaxKwh = 30, PricePerUnit = 9974, EffectiveDate = DateTime.Now, IsActive = true, Description = "Bậc 3: 21-30 m³" },
            new Tariff { ServiceType = MeterType.Water, Tier = 4, MinKwh = 31, MaxKwh = null, PricePerUnit = 14835, EffectiveDate = DateTime.Now, IsActive = true, Description = "Bậc 4: Trên 30 m³" }
        };
        context.Tariffs.AddRange(waterTariffs);

        // Seed Sample Customers
        var customers = new[]
        {
            new Customer { FullName = "Nguyễn Văn A", Address = "123 Đường ABC, Quận 1, TP.HCM", PhoneNumber = "0901234567", Email = "nguyenvana@example.com", IdentityCard = "001234567890", CreatedDate = DateTime.Now, IsActive = true },
            new Customer { FullName = "Trần Thị B", Address = "456 Đường XYZ, Quận 3, TP.HCM", PhoneNumber = "0907654321", Email = "tranthib@example.com", IdentityCard = "009876543210", CreatedDate = DateTime.Now, IsActive = true },
            new Customer { FullName = "Lê Văn C", Address = "789 Đường DEF, Quận 5, TP.HCM", PhoneNumber = "0909999888", Email = "levanc@example.com", IdentityCard = "001122334455", CreatedDate = DateTime.Now, IsActive = true }
        };
        context.Customers.AddRange(customers);

        // Save all
        context.SaveChanges();

        // Seed Meters for customers (need customer IDs first)
        var meters = new[]
        {
            new Meter { MeterNumber = "DIN001", Type = MeterType.Electric, CustomerId = customers[0].Id, InstallDate = DateTime.Now.AddMonths(-6), Location = "Tầng 1", IsActive = true },
            new Meter { MeterNumber = "NUO001", Type = MeterType.Water, CustomerId = customers[0].Id, InstallDate = DateTime.Now.AddMonths(-6), Location = "Khu vực sân", IsActive = true },
            new Meter { MeterNumber = "DIN002", Type = MeterType.Electric, CustomerId = customers[1].Id, InstallDate = DateTime.Now.AddMonths(-12), Location = "Tầng 2", IsActive = true },
            new Meter { MeterNumber = "NUO002", Type = MeterType.Water, CustomerId = customers[1].Id, InstallDate = DateTime.Now.AddMonths(-12), Location = "Tầng 1", IsActive = true },
            new Meter { MeterNumber = "DIN003", Type = MeterType.Electric, CustomerId = customers[2].Id, InstallDate = DateTime.Now.AddMonths(-3), Location = "Phòng khách", IsActive = true }
        };
        context.Meters.AddRange(meters);
        context.SaveChanges();

        Console.WriteLine("✓ Seed data hoàn tất!");
        Console.WriteLine($"  - {context.Users.Count()} users");
        Console.WriteLine($"  - {context.Customers.Count()} customers");
        Console.WriteLine($"  - {context.Meters.Count()} meters");
        Console.WriteLine($"  - {context.Tariffs.Count()} tariffs");
    }
}
