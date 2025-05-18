using System;

namespace CurrencyConverter.Dto.Currency.Response;

public class HistoricalExchangeRatesResponseDto
{
    public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = [];
}
