using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KobiPanel.Business.Services;

namespace KobiPanel.API.Controllers;

[ApiController]
[Route("api/businesses/{businessId}/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// İşletme dashboard özeti
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSummary(int businessId)
    {
        var result = await _dashboardService.GetSummaryAsync(businessId);
        return Ok(result);
    }
}