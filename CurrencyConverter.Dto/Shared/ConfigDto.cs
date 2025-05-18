using System;

namespace CurrencyConverter.Dto.Shared
{
    public class ConfigDto
    {
        public ExchangeRateProviderConfigDto ExchangeRateProviderConfig { get; set; } = new();
        public JwtConfigDto JwtConfig { get; set; } = new();
        public RateLimitConfigDto RateLimitConfig { get; set; } = new();
    }

    public class ExchangeRateProviderConfigDto
    {
        public string ClientName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string[] UnsupportedCurrencies { get; set; } = [];
    }

    public class JwtConfigDto
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationInMinutes { get; set; }
    }

    public class RateLimitConfigDto
    {
        public int MaxRequestsInWindow { get; set; }
        public int WindowInMinutes { get; set; }
    }
}
