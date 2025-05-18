using CurrencyConverter.Contract.ExchangeRate;
using CurrencyConverter.Contract.Services;
using CurrencyConverter.Dto.Currency.Request;
using CurrencyConverter.Dto.Currency.Response;
using CurrencyConverter.Dto.Shared;

namespace CurrencyConverter.Service.Currency;

/// <summary>
/// CurrencyService is an implementation of ICurrencyService that provides methods to get exchange rates and convert currencies.
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly ConfigDto _config;
    private readonly IExchangeRateProvider _exchangeRateProvider;

    public CurrencyService(ConfigDto config, IExchangeRateProviderFactory factory)
    {
        _config = config;
        _exchangeRateProvider = factory.GetProvider(_config.ExchangeRateProviderConfig.ClientName);
    }

    /// <summary>
    /// Get the latest exchange rates for a given base currency.
    /// </summary>
    /// <param name="baseCurrency"></param>
    /// <returns></returns>
    public async Task<ApiResponseDto<ExchangeRateResponseDto>> GetLatestExchangeRatesAsync(string baseCurrency)
    {
        return await _exchangeRateProvider.GetLatestExchangeRatesAsync(baseCurrency);
    }

    /// <summary>
    /// Convert currency from one to another.
    /// </summary>
    /// <param name="requestDto"></param>
    /// <returns></returns>
    public async Task<ApiResponseDto<CurrencyConversionResponseDto>> ConvertCurrencyAsync(CurrencyConversionRequestDto requestDto)
    {
        return await _exchangeRateProvider.ConvertCurrencyAsync(requestDto);
    }

    /// <summary>
    /// Get historical exchange rates for a given base currency and date range.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<PaginatedApiResponseDto<HistoricalExchangeRatesResponseDto>> GetHistoricalExchangeRatesAsync(HistoricalExchangeRatesRequestDto request)
    {
        return await _exchangeRateProvider.GetHistoricalExchangeRatesAsync(request);
    }

}
