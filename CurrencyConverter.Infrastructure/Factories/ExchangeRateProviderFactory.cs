using CurrencyConverter.Contract.ExchangeRate;
using CurrencyConverter.Infrastructure.ExchangeRateProviders;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Infrastructure.Factories;

public class ExchangeRateProviderFactory : IExchangeRateProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ExchangeRateProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Get the exchange rate provider based on the provider name.
    /// </summary>
    /// <param name="providerName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IExchangeRateProvider GetProvider(string providerName)
    {
        return providerName.ToLower() switch
        {
            "frankfurter" => _serviceProvider.GetRequiredService<FrankfurterProvider>(),
            _ => throw new ArgumentException($"No provider found for {providerName}")
        };
    }
}
