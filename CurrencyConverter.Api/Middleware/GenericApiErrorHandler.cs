using CurrencyConverter.Dto.Shared;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace CurrencyConverter.Api.Middleware;

public static class GenericApiErrorHandler
{
    /// <summary>
    /// Global exception handler for the API.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task HandleErrorAsync(HttpContext context)
    {
        HttpStatusCode status = HttpStatusCode.InternalServerError;
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";

        IExceptionHandlerFeature? contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature == null)
            return;

        ErrorDto error = new()
        {
            DisplayMessage = "An unexpected error occurred while processing your request.",
            // This is for debugging purposes only. In production, you might want to log this instead.
            ErrorMessage = contextFeature.Error.Message,
            StackTrace = contextFeature.Error.StackTrace
        };

        await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
    }
}
