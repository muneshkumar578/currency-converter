using System;

namespace CurrencyConverter.Dto.Currency.Response;

public class ExchangeRateResponseDto
{
    public string Base { get; set; } = string.Empty;
    public Dictionary<string, decimal> Rates { get; set; } = [];
}
