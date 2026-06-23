namespace IoTPlatform.API.Authentication;

/// <summary>Bound from configuration section "Jwt".</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "IoTPlatform";
    public string Audience { get; set; } = "IoTPlatform";
    public string SigningKey { get; set; } = null!;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
}
