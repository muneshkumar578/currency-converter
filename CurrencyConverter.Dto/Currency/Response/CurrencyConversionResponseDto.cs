using System;

namespace CurrencyConverter.Dto.Currency.Response;

public class CurrencyConversionResponseDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ConvertedAmount { get; set; }
}
