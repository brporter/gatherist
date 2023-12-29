namespace api.Models;

public class JwtConfiguration
{
    public JwtConfiguration(IConfiguration configuration)
    {
        SecretKey = configuration["Jwt:SecretKey"] ?? string.Empty;
        Issuer = configuration["Jwt:Issuer"] ?? string.Empty;
        Audience = configuration["Jwt:Audience"] ?? string.Empty;
        Domain = configuration["Jwt:Domain"] ?? string.Empty;

    var providers = 
            configuration.GetSection("Jwt:Providers").Get<List<JwtAuthenticationProvider>>()
            ?? Enumerable.Empty<JwtAuthenticationProvider>();

        Providers = providers.ToDictionary(p => p.Name.ToLowerInvariant());
    }
    
    public string SecretKey { get; init; }
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public string Domain { get; init; }
    public IDictionary<string, JwtAuthenticationProvider> Providers { get; init; }
}
