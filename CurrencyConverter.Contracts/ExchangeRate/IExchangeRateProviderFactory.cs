using System;

namespace CurrencyConverter.Contract.ExchangeRate;

public interface IExchangeRateProviderFactory
{
    IExchangeRateProvider GetProvider(string providerName);
}
