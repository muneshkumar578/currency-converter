using System;

namespace CurrencyConverter.Dto.Shared;

public class ErrorDto
{
    public string DisplayMessage { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
}
