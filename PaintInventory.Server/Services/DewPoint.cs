namespace PaintInventory.Server.Services;

public static class DewPoint
{
    private const double B = 17.62;
    private const double C = 243.12;

    public static decimal? Calculate(decimal? airTempC, decimal? humidityPct)
    {
        if (airTempC is null || humidityPct is null) return null;
        if (humidityPct <= 0m || humidityPct > 100m) return null;

        var t = (double)airTempC.Value;
        var rh = (double)humidityPct.Value;
        var gamma = Math.Log(rh / 100.0) + (B * t) / (C + t);
        var td = (C * gamma) / (B - gamma);
        return Math.Round((decimal)td, 1);
    }
}