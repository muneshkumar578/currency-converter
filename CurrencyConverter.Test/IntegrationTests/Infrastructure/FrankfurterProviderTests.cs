using CurrencyConverter.Dto.Currency.Request;
using CurrencyConverter.Dto.Shared;
using CurrencyConverter.Infrastructure.ExchangeRateProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace CurrencyConverter.Test.IntegrationTests.Infrastructure;

public class FrankfurterProviderTests
{
    private readonly FrankfurterProvider _frankfurterProvider;

    public FrankfurterProviderTests()
    {
        var mockHttp = new MockHttpMessageHandler();

        mockHttp.When("https://api.frankfurter.app/latest?base=EUR")
            .Respond("application/json", @"{
                ""base"": ""EUR"",
                ""date"": ""2023-10-01"",
                ""rates"": {
                    ""AUD"": 1.7458,
                    ""CAD"": 1.4608,
                    ""CHF"": 1.6051,
                    ""CYP"": 0.57667
                }
            }");

        mockHttp.When("https://api.frankfurter.app/2000-01-01..2000-12-31?base=EUR")
            .Respond("application/json", @"{
                ""base"": ""EUR"",
                ""start_date"": ""1999-12-30"",
                ""end_date"": ""2000-12-29"",
                ""rates"": {
                    ""1999-12-30"": {
                        ""AUD"": 1.5422,
                        ""CAD"": 1.4608,
                        ""CHF"": 1.6051,
                        ""CYP"": 0.57667
                    },
                    ""2000-01-03"": {
                        ""AUD"": 1.5346,
                        ""CAD"": 1.4577,
                        ""CHF"": 1.6043,
                        ""CYP"": 0.5767
                    }
                }
            }");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.frankfurter.app/");

        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);


        var config = new ConfigDto
        {
            ExchangeRateProviderConfig = new ExchangeRateProviderConfigDto
            {
                ClientName = "Frankfurter",
                UnsupportedCurrencies = ["TRY", "PLN", "THB", "MXN"],
                BaseUrl = "https://api.frankfurter.app/"
            }
        };

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<FrankfurterProvider>>();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

        _frankfurterProvider = new FrankfurterProvider(mockFactory.Object, config, memoryCache, httpContextAccessor.Object, logger);
    }

    [Fact]
    public async Task GetLatestExchangeRatesAsync_ShouldReturnExchangeRates()
    {
        // Arrange
        string baseCurrency = "EUR";

        // Act
        var result = await _frankfurterProvider.GetLatestExchangeRatesAsync(baseCurrency);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("EUR", result.Data!.Base);
        Assert.NotEmpty(result.Data.Rates);
        Assert.Contains(result.Data.Rates, x => x.Key == "AUD");
    }

    [Fact]
    public async Task ConvertCurrency_ShouldReturnConvertedAmount()
    {
        // Arrange
        var requestDto = new CurrencyConversionRequestDto
        {
            From = "EUR",
            To = "AUD",
            Amount = 100
        };

        // Act
        var result = await _frankfurterProvider.ConvertCurrencyAsync(requestDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("AUD", result.Data!.To);
        Assert.Equal(174.58m, result.Data.ConvertedAmount);
    }

    [Fact]
    public async Task GetHistoricalExchangeRatesAsync_ShouldReturnHistoricalRates()
    {
        // Arrange
        var request = new HistoricalExchangeRatesRequestDto
        {
            BaseCurrency = "EUR",
            StartDate = new DateTime(2000, 1, 1),
            EndDate = new DateTime(2000, 12, 31),
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _frankfurterProvider.GetHistoricalExchangeRatesAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEmpty(result.Data!.Rates);
        Assert.Contains(result.Data.Rates, x => x.Key == "2000-01-03");

    }
}
