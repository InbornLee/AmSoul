namespace AmSoul.Identity.MongoDB;

public sealed class JwtTokenOptions
{
    public string? SecurityKey { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public double DurationInMinutes { get; set; }
}
