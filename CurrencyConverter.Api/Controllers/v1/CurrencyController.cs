using CurrencyConverter.Contract.Services;
using CurrencyConverter.Dto.Currency.Request;
using CurrencyConverter.Dto.Currency.Response;
using CurrencyConverter.Dto.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]

/// <summary>
/// This controller handles currency related operations.
/// </summary>
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    private readonly ConfigDto _config;

    public CurrencyController(ICurrencyService currencyService, ConfigDto config)
    {
        _currencyService = currencyService;
        _config = config;
    }

    /// <summary>
    /// Get the latest exchange rates for a given base currency.
    /// </summary>
    /// <param name="baseCurrency"></param>
    /// <returns></returns>
    [HttpGet("latest-rates")]
    public async Task<IActionResult> GetLatestExchangeRates([FromQuery] string baseCurrency)
    {
        var response = await _currencyService.GetLatestExchangeRatesAsync(baseCurrency);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Convert currency from one to another.
    /// This endpoint is only accessible to Admin users.
    /// </summary>
    /// <param name="requestDto"></param>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("convert")]
    public async Task<IActionResult> ConvertCurrency([FromBody] CurrencyConversionRequestDto requestDto)
    {
        if (_config.ExchangeRateProviderConfig.UnsupportedCurrencies.Any(x => x.Equals(requestDto.From, StringComparison.OrdinalIgnoreCase)
            || x.Equals(requestDto.To, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new ApiResponseDto<CurrencyConversionResponseDto>
            {
                Success = false,
                Message = "Unsupported currency.",
                Data = null
            });
        }


        var response = await _currencyService.ConvertCurrencyAsync(requestDto);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Get historical exchange rates for a given base currency.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Roles = "Admin")]
    [HttpGet("historical-rates")]
    public async Task<IActionResult> GetHistoricalExchangeRates([FromQuery] HistoricalExchangeRatesRequestDto request)
    {
        var response = await _currencyService.GetHistoricalExchangeRatesAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}
