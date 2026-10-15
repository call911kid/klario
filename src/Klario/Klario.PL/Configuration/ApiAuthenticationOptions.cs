namespace Klario.PL.Configuration;

public sealed class ApiAuthenticationOptions
{
    public const string SectionName = "Authentication";
    public string ApiKey { get; set; } = string.Empty;
}
