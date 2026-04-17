using System.Text.Json;
using FaturamentoService.Application.Exceptions;
using FaturamentoService.Domain.Exceptions;

namespace FaturamentoService.Api.Middleware;

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
            ExternalServiceException => StatusCodes.Status503ServiceUnavailable,
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
                StatusCodes.Status500InternalServerError => "Ocorreu um erro inesperado.",
                StatusCodes.Status503ServiceUnavailable => "Nao foi possivel emitir a nota fiscal agora. Tente novamente mais tarde.",
                _ => exception.Message
            },
            traceId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
