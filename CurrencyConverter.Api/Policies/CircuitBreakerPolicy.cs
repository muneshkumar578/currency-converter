using Polly.Extensions.Http;
using Polly;

namespace CurrencyConverter.Api.Policies;

public static class CircuitBreakerPolicy
{
    /// <summary>
    /// Circuit breaker policy for HTTP requests.
    /// </summary>
    /// <returns></returns>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));
    }
}
