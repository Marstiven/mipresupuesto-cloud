using System.Net;
using System.Text.Json;

namespace MiPresupuesto.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no controlada durante el procesamiento de la solicitud HTTP: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var respuesta = new
        {
            exito = false,
            mensaje = "Ha ocurrido un error interno en el servidor. Por favor, inténtelo de nuevo más tarde.",
            detalle = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true 
                ? exception.Message 
                : null,
            status = context.Response.StatusCode,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(respuesta, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

