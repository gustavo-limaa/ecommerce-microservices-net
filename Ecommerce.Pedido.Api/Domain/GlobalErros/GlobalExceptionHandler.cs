using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using applicate = Ecommerce.Pedido.Api.Domain.GlobalErros.ApplicationException;

namespace Ecommerce.Pedido.Api.Domain.GlobalErros;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exceção capturada: {Message}", exception.Message);

        var (statusCode, title) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Erro de Validação de Negócio"),
            applicate => (StatusCodes.Status400BadRequest, "Erro na Aplicação"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso Não Encontrado"),
            _ => (StatusCodes.Status500InternalServerError, "Erro Interno no Servidor")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}