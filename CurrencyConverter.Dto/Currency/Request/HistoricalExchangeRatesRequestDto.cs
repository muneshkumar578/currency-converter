using System;

namespace CurrencyConverter.Dto.Currency.Request;

public class HistoricalExchangeRatesRequestDto
{
    public string BaseCurrency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
