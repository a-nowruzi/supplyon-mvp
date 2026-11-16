using O2Ban.EnterprisePilot.Models;

namespace O2Ban.EnterprisePilot.Services;

public static class SupplierPerformanceCalculator
{
    public static int CalculateDeliveryScore(DateTime expected, DateTime? actual)
    {
        if (!actual.HasValue) return 0;
        var lateDays = (actual.Value.Date - expected.Date).Days;
        return lateDays <= 0 ? 5 : lateDays <= 3 ? 4 : lateDays <= 7 ? 3 : lateDays <= 14 ? 2 : 1;
    }

    public static decimal CalculateFulfillmentRate(decimal ordered, decimal accepted)
    {
        if (ordered <= 0) return 0;
        return Math.Clamp(accepted / ordered * 100m, 0m, 100m);
    }

    public static (decimal OverallScore, int DeliveryScore, decimal FulfillmentRate) Calculate(ProcurementOrder order, int quality, int communication, int documentation)
    {
        var delivery = CalculateDeliveryScore(order.ExpectedDeliveryDate, order.ActualDeliveryDate);
        var fulfillment = CalculateFulfillmentRate(order.OrderedQuantity, order.AcceptedQuantity);
        // Weighted score: quality 40%, delivery 25%, accepted quantity fulfillment 15%, communication 10%, documentation 10%.
        var score = (quality / 5m * 40m)
                    + (delivery / 5m * 25m)
                    + (fulfillment / 100m * 15m)
                    + (communication / 5m * 10m)
                    + (documentation / 5m * 10m);
        return (Math.Round(score, 2), delivery, Math.Round(fulfillment, 2));
    }
}
