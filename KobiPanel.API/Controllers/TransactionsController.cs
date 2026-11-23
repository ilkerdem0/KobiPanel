using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KobiPanel.Business.Services;
using KobiPanel.API.Filters;

namespace KobiPanel.API.Controllers;

[ApiController]
[Route("api/businesses/{businessId}/[controller]")]
[Authorize]
[BusinessOwner]
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
        int? CategoryId, int? CustomerId,
        bool IsScheduled, DateTime? DueDate
    );

    [HttpPost]
    public async Task<IActionResult> Create(int businessId, [FromBody] CreateTransactionRequest request)
    {
        var result = await _transactionService.CreateAsync(
            businessId, request.Description, request.Amount,
            request.Type, request.TransactionDate, request.PaymentMethod,
            request.CategoryId, request.CustomerId,
            request.IsScheduled, request.DueDate);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int businessId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? type = null, [FromQuery] string? paymentStatus = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var result = await _transactionService.GetAllAsync(businessId, page, pageSize, type, paymentStatus, from, to);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(int businessId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _transactionService.GetSummaryAsync(businessId, from, to);
        return Ok(result);
    }

    [HttpGet("chart")]
    public async Task<IActionResult> GetChart(int businessId, [FromQuery] int days = 30)
    {
        var result = await _transactionService.GetDailyChartAsync(businessId, days);
        return Ok(result);
    }

    [HttpPut("{transactionId}/complete")]
    public async Task<IActionResult> MarkAsCompleted(int businessId, int transactionId)
    {
        var result = await _transactionService.MarkAsCompletedAsync(businessId, transactionId);
        return Ok(result);
    }

    [HttpDelete("{transactionId}")]
    public async Task<IActionResult> Delete(int businessId, int transactionId)
    {
        await _transactionService.DeleteAsync(businessId, transactionId);
        return Ok(new { message = "İşlem silindi" });
    }
}