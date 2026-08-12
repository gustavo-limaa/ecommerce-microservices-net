using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Ecommerce.Catalogo.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Catalogo.Api.Domain.GlobalErros.ErrosMassege;

namespace Ecommerce.Catalogo.Api.Domain.GlobalErros;

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
            NotFoundException => (StatusCodes.Status404NotFound, ApplicationMessages.NaoEncontrado),
            ConflictException => (StatusCodes.Status409Conflict, ApplicationMessages.Conflito),
            BadRequestException => (StatusCodes.Status400BadRequest, ApplicationMessages.DadosInvalidos),
            DomainException => (StatusCodes.Status400BadRequest, ApplicationMessages.DadosInvalidos),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, ApplicationMessages.SemAutorizacao),
            ForbiddenException => (StatusCodes.Status403Forbidden, ApplicationMessages.SemPermissao),

            // Fallback para exceções não tratadas
            _ => (StatusCodes.Status500InternalServerError, ApplicationMessages.ErroInesperado)
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