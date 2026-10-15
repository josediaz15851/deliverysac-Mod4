namespace DeliverySac.Api.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "DeliverySac";
    public string Audience { get; set; } = "DeliverySac";
    public string Key { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 480;
}
