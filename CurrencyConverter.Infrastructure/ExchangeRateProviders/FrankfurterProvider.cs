using CurrencyConverter.Contract.ExchangeRate;
using CurrencyConverter.Dto.Currency.Request;
using CurrencyConverter.Dto.Currency.Response;
using CurrencyConverter.Dto.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CurrencyConverter.Infrastructure.ExchangeRateProviders;

/// <summary>
/// FrankfurterProvider is an implementation of IExchangeRateProvider that uses the Frankfurter API to get exchange rates.
/// Any unhandled error thrown in this class will be captured by global exception handler.
/// This provider uses a retry with exponential backoff and a circuit breaker policy.
/// </summary>
public class FrankfurterProvider : IExchangeRateProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConfigDto _config;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<FrankfurterProvider> _logger;

    public FrankfurterProvider(IHttpClientFactory httpClientFactory, ConfigDto config, IMemoryCache cache, IHttpContextAccessor accessor,
        ILogger<FrankfurterProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _cache = cache;
        _accessor = accessor;
        _logger = logger;
    }

    #region Private Methods

    /// <summary>
    /// Frankfurter API Client with policies for retry and circuit breaker added to it.
    /// </summary>
    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient(_config.ExchangeRateProviderConfig.ClientName);
    }


    private string LatestRatesEnpoint => "latest?base={0}";


    private static string LatestRatesCacheKey(string baseCurrency)
    {
        return $"LatestRates-{baseCurrency.ToLower()}";
    }

    private static string ConvertCurrencyCacheKey(string from, string to, decimal amount)
    {
        return $"ConvertCurrency-{from.ToLower()}-{to.ToLower()}-{amount}";
    }

    private static string HistoricalExchangeRatesCacheKey(string baseCurrency, DateTime start, DateTime end)
    {
        return $"HistoricalExchangeRates-{baseCurrency.ToLower()}-{start:yyyy-MM-dd}-{end:yyyy-MM-dd}";
    }


    /// <summary>
    /// Set cache key for the given data.
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="data"></param>
    /// <param name="expiryInMinutes"></param>
    private void SetCacheKey(string cacheKey, object? data, int expiryInMinutes = 5)
    {
        _cache.Set(cacheKey, data, TimeSpan.FromMinutes(expiryInMinutes));
    }

    private static void PopulatePaginatedData(PaginatedApiResponseDto<HistoricalExchangeRatesResponseDto> result, HistoricalExchangeRatesResponseDto ratesResponse, int page, int pageSize)
    {
        int skip = (page - 1) * pageSize;
        int take = pageSize;

        var paginatedRates = ratesResponse.Rates.Skip(skip).Take(take).ToDictionary(x => x.Key, x => x.Value);

        result.Page = page;
        result.PageSize = pageSize;
        result.Total = ratesResponse.Rates.Count;
        result.Data = new HistoricalExchangeRatesResponseDto
        {
            Rates = paginatedRates
        };
    }

    private string GetCorrelationId()
    {
        return _accessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? string.Empty;
    }

    #endregion


    #region Public Methods


    /// <summary>
    /// Get the latest exchange rates for a given base currency.
    /// </summary>
    /// <param name="baseCurrency"></param>
    /// <returns></returns>
    public async Task<ApiResponseDto<ExchangeRateResponseDto>> GetLatestExchangeRatesAsync(string baseCurrency)
    {
        _logger.LogInformation("CorrelationId: {GetCorrelationId} - Fetching latest exchange rates for base currency: {baseCurrency}", GetCorrelationId(), baseCurrency);

        var resultSuccess = new ApiResponseDto<ExchangeRateResponseDto> { Success = true, Message = "Successful." };

        if (_cache.TryGetValue(LatestRatesCacheKey(baseCurrency), out var cachedRatesObj) && cachedRatesObj is ExchangeRateResponseDto cachedRates)
        {
            resultSuccess.Data = cachedRates;
            return resultSuccess;
        }


        var client = CreateClient();
        var response = await client.GetAsync(string.Format(LatestRatesEnpoint, baseCurrency));
        

        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponseDto<ExchangeRateResponseDto>
            {
                Success = false,
                Message = "Failed to fetch exchange rates.",
            };
        }

        var content = await response.Content.ReadAsStringAsync();

        //Deseralize the response
        var exchangeRateDto = JsonConvert.DeserializeObject<ExchangeRateResponseDto>(content);

        resultSuccess.Data = exchangeRateDto;

        SetCacheKey(LatestRatesCacheKey(baseCurrency), exchangeRateDto);

        return resultSuccess;
    }

    /// <summary>
    /// Convert currency from one to another.
    /// </summary>
    /// <param name="requestDto"></param>
    /// <returns></returns>
    public async Task<ApiResponseDto<CurrencyConversionResponseDto>> ConvertCurrencyAsync(CurrencyConversionRequestDto requestDto)
    {
        _logger.LogInformation("CorrelationId: {GetCorrelationId} - Converting currency from {From} to {To} with amount {Amount}", GetCorrelationId(), requestDto.From, requestDto.To, requestDto.Amount);

        var resultSuccess = new ApiResponseDto<CurrencyConversionResponseDto> { Success = true, Message = "Successful." };

        if (_cache.TryGetValue(ConvertCurrencyCacheKey(requestDto.From, requestDto.To, requestDto.Amount), out var cachedRatesObj) && cachedRatesObj is CurrencyConversionResponseDto cachedRates)
        {
            resultSuccess.Data = cachedRates;
            return resultSuccess;
        }

        var client = CreateClient();
        var response = await client.GetAsync(string.Format(LatestRatesEnpoint, requestDto.From));

        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponseDto<CurrencyConversionResponseDto>
            {
                Success = false,
                Message = "Failed to fetch exchange rates.",
            };
        }

        var content = await response.Content.ReadAsStringAsync();

        var exchangeRateDto = JsonConvert.DeserializeObject<ExchangeRateResponseDto>(content);

        if (exchangeRateDto == null || !exchangeRateDto.Rates.TryGetValue(requestDto.To, out decimal rate))
        {
            return new ApiResponseDto<CurrencyConversionResponseDto>
            {
                Success = false,
                Message = "Currency not supported.",
            };
        }

        var convertedAmount = requestDto.Amount * rate;

        var conversionResponse = new CurrencyConversionResponseDto
        {
            From = requestDto.From,
            To = requestDto.To,
            Amount = requestDto.Amount,
            ConvertedAmount = convertedAmount
        };

        resultSuccess.Data = conversionResponse;
        SetCacheKey(ConvertCurrencyCacheKey(requestDto.From, requestDto.To, requestDto.Amount), conversionResponse);

        return resultSuccess;
    }

    /// <summary>
    /// Get historical exchange rates for a given base currency and date range.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<PaginatedApiResponseDto<HistoricalExchangeRatesResponseDto>> GetHistoricalExchangeRatesAsync(HistoricalExchangeRatesRequestDto request)
    {
        _logger.LogInformation("CorrelationId: {GetCorrelationId} - Fetching historical exchange rates for base currency: {BaseCurrency} from {StartDate} to {EndDate}", GetCorrelationId(), request.BaseCurrency, request.StartDate, request.EndDate);

        var resultSuccess = new PaginatedApiResponseDto<HistoricalExchangeRatesResponseDto> { Success = true, Message = "Successful." };

        if (_cache.TryGetValue(HistoricalExchangeRatesCacheKey(request.BaseCurrency, request.StartDate, request.EndDate), out var cachedRatesObj) && cachedRatesObj is HistoricalExchangeRatesResponseDto cachedRates)
        {
            PopulatePaginatedData(result: resultSuccess, ratesResponse: cachedRates, page: request.Page, pageSize: request.PageSize);
            return resultSuccess;
        }

        var client = CreateClient();
        var response = await client.GetAsync($"{request.StartDate:yyyy-MM-dd}..{request.EndDate:yyyy-MM-dd}?base={request.BaseCurrency}");

        if (!response.IsSuccessStatusCode)
        {
            return new PaginatedApiResponseDto<HistoricalExchangeRatesResponseDto>
            {
                Success = false,
                Message = "Failed to fetch historical exchange rates.",
            };
        }

        var content = await response.Content.ReadAsStringAsync();
        var historicalExchangeRatesDto = JsonConvert.DeserializeObject<HistoricalExchangeRatesResponseDto>(content);

        if (historicalExchangeRatesDto == null)
        {
            return new PaginatedApiResponseDto<HistoricalExchangeRatesResponseDto>
            {
                Success = false,
                Message = "Failed to parse historical exchange rates.",
            };
        }

        SetCacheKey(HistoricalExchangeRatesCacheKey(request.BaseCurrency, request.StartDate, request.EndDate), historicalExchangeRatesDto);

        PopulatePaginatedData(result: resultSuccess, ratesResponse: historicalExchangeRatesDto, page: request.Page, pageSize: request.PageSize);

        return resultSuccess;

    }

    #endregion
}
