using CurrencyConverter.Dto.Currency.Request;
using CurrencyConverter.Dto.Currency.Response;
using CurrencyConverter.Dto.Shared;

namespace CurrencyConverter.Contract.Services;

public interface ICurrencyService
{
    Task<ApiResponseDto<ExchangeRateResponseDto>> GetLatestExchangeRatesAsync(string baseCurrency);

    Task<ApiResponseDto<CurrencyConversionResponseDto>> ConvertCurrencyAsync(CurrencyConversionRequestDto requestDto);

    Task<PaginatedApiResponseDto<HistoricalExchangeRatesResponseDto>> GetHistoricalExchangeRatesAsync(HistoricalExchangeRatesRequestDto request);
}
