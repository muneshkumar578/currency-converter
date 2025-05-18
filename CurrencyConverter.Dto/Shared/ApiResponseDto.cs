using System;

namespace CurrencyConverter.Dto.Shared;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; } = default;
}
public class PaginatedApiResponseDto<T> : ApiResponseDto<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}
