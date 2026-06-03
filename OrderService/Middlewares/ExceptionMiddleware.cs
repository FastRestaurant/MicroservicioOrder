using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Exceptions;

namespace OrderService.Presentation.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning("Concurrency conflict: {msg}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Conflict,
                "The record was modified by another user. Please reload and try again.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteErrorAsync(HttpContext ctx, HttpStatusCode code, string message)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)code;
        var body = JsonSerializer.Serialize(new { error = message, statusCode = (int)code });
        return ctx.Response.WriteAsync(body);
    }
}
