using CurrencyConverter.Contract.ExchangeRate;
using CurrencyConverter.Contract.Services;
using CurrencyConverter.Dto.Currency.Request;
using CurrencyConverter.Dto.Currency.Response;
using CurrencyConverter.Dto.Shared;

namespace CurrencyConverter.Service.Currency;

public class CurrencyService : ICurrencyService
{
    private readonly ConfigDto _config;
    private readonly IExchangeRateProvider _exchangeRateProvider;

    public CurrencyService(ConfigDto config, IExchangeRateProviderFactory factory)
    {
        _config = config;
        _exchangeRateProvider = factory.GetProvider(_config.ExchangeRateProviderConfig.ClientName);
    }

    public async Task<ApiResponseDto<ExchangeRateResponseDto>> GetLatestExchangeRatesAsync(string baseCurrency)
    {
        return await _exchangeRateProvider.GetLatestExchangeRatesAsync(baseCurrency);
    }

    public async Task<ApiResponseDto<CurrencyConversionResponseDto>> ConvertCurrencyAsync(CurrencyConversionRequestDto requestDto)
    {
        return await _exchangeRateProvider.ConvertCurrencyAsync(requestDto);
    }

    public async Task<PaginatedApiResponseDto<HistoricalExchangeRatesResponseDto>> GetHistoricalExchangeRatesAsync(HistoricalExchangeRatesRequestDto request)
    {
        return await _exchangeRateProvider.GetHistoricalExchangeRatesAsync(request);
    }

}
