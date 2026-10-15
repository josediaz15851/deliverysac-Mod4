namespace DeliverySac.Api.Application;

public static class FechaOperativa
{
    private static readonly TimeZoneInfo ZonaLima = ResolverZona();

    public static DateOnly Hoy()
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaLima));
    }

    private static TimeZoneInfo ResolverZona()
    {
        string[] ids = ["America/Lima", "SA Pacific Standard Time"];

        foreach (var id in ids)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}
