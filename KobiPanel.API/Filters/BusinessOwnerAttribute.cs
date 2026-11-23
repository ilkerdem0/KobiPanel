using System.Security.Claims;
using KobiPanel.Business.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KobiPanel.API.Filters;

/// <summary>
/// Route'taki {businessId} değerinin giriş yapan kullanıcıya ait olduğunu doğrular.
/// Sahip değilse işletmenin varlığını sızdırmamak için 404 döner.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BusinessOwnerAttribute : TypeFilterAttribute
{
    public BusinessOwnerAttribute() : base(typeof(BusinessOwnerFilter)) { }
}

public class BusinessOwnerFilter : IAsyncActionFilter
{
    private readonly BusinessService _businessService;

    public BusinessOwnerFilter(BusinessService businessService)
    {
        _businessService = businessService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!int.TryParse(context.RouteData.Values["businessId"]?.ToString(), out var businessId))
        {
            context.Result = new BadRequestObjectResult(new { message = "Geçersiz işletme." });
            return;
        }

        if (!int.TryParse(context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!await _businessService.IsOwnerAsync(userId, businessId))
        {
            context.Result = new NotFoundObjectResult(new { message = "İşletme bulunamadı." });
            return;
        }

        await next();
    }
}
