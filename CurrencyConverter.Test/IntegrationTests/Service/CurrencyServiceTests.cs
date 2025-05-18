using CurrencyConverter.Contract.ExchangeRate;
using CurrencyConverter.Dto.Currency.Request;
using CurrencyConverter.Dto.Currency.Response;
using CurrencyConverter.Dto.Shared;
using CurrencyConverter.Service.Currency;
using Moq;

namespace CurrencyConverter.Test.IntegrationTests.Service;

/// <summary>
/// Unit tests for the CurrencyService class.
/// </summary>
public class CurrencyServiceTests
{
    private readonly Mock<IExchangeRateProvider> _mockIExchangeRateProvider;
    private readonly CurrencyService _currencyService;
    private readonly ConfigDto _config;

    public CurrencyServiceTests()
    {
        _mockIExchangeRateProvider = new Mock<IExchangeRateProvider>();

        var mockIExchangeRateProviderFactory = new Mock<IExchangeRateProviderFactory>();
        mockIExchangeRateProviderFactory.Setup(x => x.GetProvider(It.IsAny<string>()))
            .Returns(_mockIExchangeRateProvider.Object);

        _config = new ConfigDto
        {
            ExchangeRateProviderConfig = new ExchangeRateProviderConfigDto
            {
                ClientName = "Frankfurter",
                UnsupportedCurrencies = ["TRY", "PLN", "THB", "MXN"],
                BaseUrl = "https://api.frankfurter.app/"
            }
        };

        _currencyService = new CurrencyService(_config, mockIExchangeRateProviderFactory.Object);
    }

    [Fact]
    public async Task GetLatestExchangeRatesAsync_ShouldReturnExchangeRates_WhenCalled()
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

        _mockIExchangeRateProvider.Setup(x => x.GetLatestExchangeRatesAsync(baseCurrency))
            .ReturnsAsync(response);

        // Act
        var result = await _currencyService.GetLatestExchangeRatesAsync(baseCurrency);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(response.Data.Rates, result.Data!.Rates);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_ShouldReturnConvertedAmount_WhenCalled()
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
                ConvertedAmount = 89.5m
            }
        };

        _mockIExchangeRateProvider.Setup(x => x.ConvertCurrencyAsync(requestDto))
            .ReturnsAsync(response);

        // Act
        var result = await _currencyService.ConvertCurrencyAsync(requestDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(response.Data!.ConvertedAmount, result.Data!.ConvertedAmount);
    }

    [Fact]
    public async Task GetHistoricalExchangeRatesAsync_ShouldReturnHistoricalRates_WhenCalled()
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

        _mockIExchangeRateProvider.Setup(x => x.GetHistoricalExchangeRatesAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _currencyService.GetHistoricalExchangeRatesAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(response.Data!.Rates, result.Data!.Rates);
    }
}
