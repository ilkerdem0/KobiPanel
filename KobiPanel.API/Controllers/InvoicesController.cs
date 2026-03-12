using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KobiPanel.Business.Services;

namespace KobiPanel.API.Controllers;

[ApiController]
[Route("api/businesses/{businessId}/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public InvoicesController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public record CreateInvoiceRequest(
        int? CustomerId, DateTime? DueDate, decimal TaxRate,
        string? Notes, List<InvoiceItemInput> Items
    );

    public record UpdateStatusRequest(string Status);

    [HttpPost]
    public async Task<IActionResult> Create(int businessId, [FromBody] CreateInvoiceRequest request)
    {
        try
        {
            var result = await _invoiceService.CreateAsync(
                businessId, request.CustomerId, request.DueDate,
                request.TaxRate, request.Notes, request.Items);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int businessId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var result = await _invoiceService.GetAllAsync(businessId, page, pageSize, status);
        return Ok(result);
    }

    [HttpGet("{invoiceId}")]
    public async Task<IActionResult> GetById(int businessId, int invoiceId)
    {
        try
        {
            var result = await _invoiceService.GetByIdAsync(businessId, invoiceId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{invoiceId}/status")]
    public async Task<IActionResult> UpdateStatus(int businessId, int invoiceId, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var result = await _invoiceService.UpdateStatusAsync(businessId, invoiceId, request.Status);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{invoiceId}")]
    public async Task<IActionResult> Delete(int businessId, int invoiceId)
    {
        try
        {
            await _invoiceService.DeleteAsync(businessId, invoiceId);
            return Ok(new { message = "Fatura silindi" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}