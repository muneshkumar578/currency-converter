using CurrencyConverter.Api.Controllers.v1;
using CurrencyConverter.Contract.Services;
using CurrencyConverter.Dto.Currency.Request;
using CurrencyConverter.Dto.Currency.Response;
using CurrencyConverter.Dto.Shared;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CurrencyConverter.Test.IntegrationTests.Controller;

public class CurrencyControllerTests
{
    private readonly Mock<ICurrencyService> _mockCurrencyService;
    private readonly CurrencyController _controller;
    private readonly ConfigDto _config;

    public CurrencyControllerTests()
    {
        _mockCurrencyService = new Mock<ICurrencyService>();
        _config = new ConfigDto
        {
            ExchangeRateProviderConfig = new ExchangeRateProviderConfigDto
            {
                ClientName = "Frankfurter",
                UnsupportedCurrencies = ["TRY", "PLN", "THB", "MXN"],
                BaseUrl = "https://api.frankfurter.app/"
            }
        };
        _controller = new CurrencyController(_mockCurrencyService.Object, _config);
    }

    [Fact]
    public async Task GetLatestExchangeRates_ShouldReturnOk_WithValidResponse()
    {
        // Arrange
        var baseCurrency = "EUR";

        var response = new ApiResponseDto<ExchangeRateResponseDto>
        {
            Success = true,
            Data = new ExchangeRateResponseDto
            {
                Rates = new Dictionary<string, decimal>
                {
                      {"USD", 1.1m },
                }
            }
        };

        _mockCurrencyService.Setup(s => s.GetLatestExchangeRatesAsync(baseCurrency)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetLatestExchangeRates(baseCurrency);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponseDto<ExchangeRateResponseDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(response.Data.Rates, apiResponse.Data!.Rates);
    }

    [Fact]
    public async Task ConvertCurrency_ShouldReturnOk_WithValidResponse()
    {
        // Arrange
        var requestDto = new CurrencyConversionRequestDto
        {
            From = "USD",
            To = "EUR",
            Amount = 100
        };

        var response = new ApiResponseDto<CurrencyConversionResponseDto>
        {
            Success = true,
            Data = new CurrencyConversionResponseDto
            {
                From = "USD",
                To = "EUR",
                Amount = 100,
                ConvertedAmount = 89.5m
            }
        };

        _mockCurrencyService.Setup(s => s.ConvertCurrencyAsync(requestDto)).ReturnsAsync(response);

        // Act
        var result = await _controller.ConvertCurrency(requestDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponseDto<CurrencyConversionResponseDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(response.Data!.ConvertedAmount, apiResponse.Data!.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertCurrency_ShouldReturnBadRequest_WhenUnsupportedCurrency()
    {
        // Arrange
        var requestDto = new CurrencyConversionRequestDto
        {
            From = "USD",
            To = "TRY",
            Amount = 100
        };

        // Act
        var result = await _controller.ConvertCurrency(requestDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponseDto<CurrencyConversionResponseDto>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
    }


    [Fact]
    public async Task GetHistoricalExchangeRates_ShouldReturnOk_WithValidResponse()
    {
        // Arrange
        var request = new HistoricalExchangeRatesRequestDto
        {
            BaseCurrency = "USD",
            StartDate = new DateTime(2025, 4, 1),
            EndDate = new DateTime(2025, 4, 30),
        };

        var response = new PaginatedApiResponseDto<HistoricalExchangeRatesResponseDto>
        {
            Success = true,
            Data = new HistoricalExchangeRatesResponseDto
            {
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { "2025-04-01", new Dictionary<string, decimal> { { "EUR", 0.85m } } },
                    { "2025-04-02", new Dictionary<string, decimal> { { "EUR", 0.86m } } }
                }
            }
        };

        _mockCurrencyService.Setup(s => s.GetHistoricalExchangeRatesAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetHistoricalExchangeRates(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<PaginatedApiResponseDto<HistoricalExchangeRatesResponseDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(response.Data!.Rates, apiResponse.Data!.Rates);
    }
}
