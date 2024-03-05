using Microsoft.Extensions.Configuration;

namespace Services;

public class ConfigService
{
    private readonly IConfiguration _configuration;

    public ConfigService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string GetApiUrl(string relativePath)
    {
        var baseUrl = _configuration["ApiSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("API base URL is not configured.");
        }

        return $"{baseUrl}{relativePath}";
    }
}