using QL_HethongDiennuoc.Models.Entities;

namespace QL_HethongDiennuoc.Helpers;

public static class TariffCalculator
{
    public static decimal CalculateBill(decimal consumption, MeterType serviceType, List<Tariff> tariffs)
    {
        var applicableTariffs = tariffs
            .Where(t => t.ServiceType == serviceType && t.IsActive)
            .OrderBy(t => t.Tier)
            .ToList();

        decimal totalAmount = 0;
        decimal remainingConsumption = consumption;

        foreach (var tariff in applicableTariffs)
        {
            if (remainingConsumption <= 0)
                break;

            decimal tierMin = tariff.MinKwh;
            decimal? tierMax = tariff.MaxKwh;

            decimal tierConsumption;
            if (tierMax.HasValue)
            {
                // Calculate consumption in this tier
                decimal tierRange = tierMax.Value - tierMin;
                tierConsumption = Math.Min(remainingConsumption, tierRange);
            }
            else
            {
                // Unlimited tier (last tier)
                tierConsumption = remainingConsumption;
            }

            totalAmount += tierConsumption * tariff.PricePerUnit;
            remainingConsumption -= tierConsumption;
        }

        return Math.Round(totalAmount, 0); // Round to nearest VND
    }

    public static List<TierBreakdown> GetTierBreakdown(decimal consumption, MeterType serviceType, List<Tariff> tariffs)
    {
        var applicableTariffs = tariffs
            .Where(t => t.ServiceType == serviceType && t.IsActive)
            .OrderBy(t => t.Tier)
            .ToList();

        var breakdown = new List<TierBreakdown>();
        decimal remainingConsumption = consumption;

        foreach (var tariff in applicableTariffs)
        {
            if (remainingConsumption <= 0)
                break;

            decimal tierMin = tariff.MinKwh;
            decimal? tierMax = tariff.MaxKwh;

            decimal tierConsumption;
            if (tierMax.HasValue)
            {
                decimal tierRange = tierMax.Value - tierMin;
                tierConsumption = Math.Min(remainingConsumption, tierRange);
            }
            else
            {
                tierConsumption = remainingConsumption;
            }

            breakdown.Add(new TierBreakdown
            {
                Tier = tariff.Tier,
                Description = tariff.Description ?? $"Bậc {tariff.Tier}",
                Consumption = tierConsumption,
                PricePerUnit = tariff.PricePerUnit,
                Amount = tierConsumption * tariff.PricePerUnit
            });

            remainingConsumption -= tierConsumption;
        }

        return breakdown;
    }
}

public class TierBreakdown
{
    public int Tier { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Consumption { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal Amount { get; set; }
}
