using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KobiPanel.Business.Services;

namespace KobiPanel.API.Controllers;

[ApiController]
[Route("api/businesses/{businessId}/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public record CreateTransactionRequest(
        string Description, decimal Amount, string Type,
        DateTime TransactionDate, string? PaymentMethod,
        int? CategoryId, int? CustomerId
    );

    /// <summary>
    /// Yeni gelir/gider ekle
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(int businessId, [FromBody] CreateTransactionRequest request)
    {
        try
        {
            var result = await _transactionService.CreateAsync(
                businessId, request.Description, request.Amount,
                request.Type, request.TransactionDate, request.PaymentMethod,
                request.CategoryId, request.CustomerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// İşlem listesi (filtreli + sayfalı)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(int businessId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? type = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var result = await _transactionService.GetAllAsync(businessId, page, pageSize, type, from, to);
        return Ok(result);
    }

    /// <summary>
    /// Gelir-gider özeti (belirli tarih aralığı)
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(int businessId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _transactionService.GetSummaryAsync(businessId, from, to);
        return Ok(result);
    }

    /// <summary>
    /// İşlem sil
    /// </summary>
    [HttpDelete("{transactionId}")]
    public async Task<IActionResult> Delete(int businessId, int transactionId)
    {
        try
        {
            await _transactionService.DeleteAsync(businessId, transactionId);
            return Ok(new { message = "İşlem silindi" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}