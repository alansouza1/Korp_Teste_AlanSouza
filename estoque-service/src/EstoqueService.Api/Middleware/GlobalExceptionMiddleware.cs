using System.Text.Json;
using EstoqueService.Application.Exceptions;
using EstoqueService.Domain.Exceptions;

namespace EstoqueService.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            DomainException => StatusCodes.Status400BadRequest,
            SimulatedFailureException => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception. TraceId: {TraceId}", traceId);
        }

        var payload = new
        {
            message = statusCode switch
            {
                StatusCodes.Status500InternalServerError => "An unexpected error occurred.",
                StatusCodes.Status503ServiceUnavailable => "Unable to process stock right now. Please try again later.",
                _ => exception.Message
            },
            errorCode = exception switch
            {
                SimulatedFailureException => "ESTOQUE_SIMULATED_FAILURE",
                _ => null
            },
            traceId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
