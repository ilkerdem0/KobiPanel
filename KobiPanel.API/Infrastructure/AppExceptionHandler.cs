using KobiPanel.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace KobiPanel.API.Infrastructure;

/// <summary>
/// Tüm yakalanmamış exception'ları { message } JSON'una çevirir.
/// AppException türevleri kendi status kodu ve mesajıyla döner; diğerleri loglanır
/// ve iç detay sızdırılmadan 500 olarak döner.
/// </summary>
public class AppExceptionHandler : IExceptionHandler
{
    private readonly ILogger<AppExceptionHandler> _logger;

    public AppExceptionHandler(ILogger<AppExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode;
        string message;

        if (exception is AppException appException)
        {
            statusCode = appException.StatusCode;
            message = appException.Message;
        }
        else
        {
            _logger.LogError(exception, "Beklenmeyen hata: {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
            statusCode = StatusCodes.Status500InternalServerError;
            message = "Beklenmeyen bir hata oluştu.";
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new { message }, cancellationToken);
        return true;
    }
}
