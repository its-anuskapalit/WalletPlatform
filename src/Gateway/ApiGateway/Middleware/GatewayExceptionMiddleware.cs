using System.Net;
using System.Text.Json;
using WalletPlatform.Shared.Models;

namespace ApiGateway.Middleware;

public class GatewayExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GatewayExceptionMiddleware> _logger;

    public GatewayExceptionMiddleware(
        RequestDelegate next,
        ILogger<GatewayExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // Handle Ocelot-level 401/403 cleanly
            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized
                && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var response = ApiResponse<object>.Fail(
                    "Unauthorized. Please provide a valid token.");
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));
            }

            if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden
                && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var response = ApiResponse<object>.Fail(
                    "Forbidden. You do not have permission to access this resource.");
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled gateway exception");
            context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var response = ApiResponse<object>.Fail("Gateway error. Please try again.");
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
        }
    }
}