using Polly;

namespace CurrencyConverter.Api.Policies;

public static class RetryPolicy
{
    /// <summary>
    /// Retry policy for HTTP requests.
    /// </summary>
    /// <returns></returns>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
