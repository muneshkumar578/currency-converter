using System;

namespace CurrencyConverter.Dto.Currency.Request;

public class CurrencyConversionRequestDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
