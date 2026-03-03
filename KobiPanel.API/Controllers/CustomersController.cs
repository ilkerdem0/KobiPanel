using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KobiPanel.Business.Services;

namespace KobiPanel.API.Controllers;

[ApiController]
[Route("api/businesses/{businessId}/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;

    public CustomersController(CustomerService customerService)
    {
        _customerService = customerService;
    }

    public record CreateCustomerRequest(
        string FirstName, string LastName,
        string? PhoneNumber, string? Email, string? Notes
    );

    /// <summary>
    /// Yeni müşteri ekle
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(int businessId, [FromBody] CreateCustomerRequest request)
    {
        try
        {
            var result = await _customerService.CreateAsync(
                businessId, request.FirstName, request.LastName,
                request.PhoneNumber, request.Email, request.Notes);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Müşteri listesi (arama + sayfalama)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(int businessId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _customerService.GetAllAsync(businessId, page, pageSize, search);
        return Ok(result);
    }

    /// <summary>
    /// Müşteri detayı
    /// </summary>
    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetById(int businessId, int customerId)
    {
        try
        {
            var result = await _customerService.GetByIdAsync(businessId, customerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Müşteri güncelle
    /// </summary>
    [HttpPut("{customerId}")]
    public async Task<IActionResult> Update(int businessId, int customerId, [FromBody] CreateCustomerRequest request)
    {
        try
        {
            var result = await _customerService.UpdateAsync(
                businessId, customerId, request.FirstName, request.LastName,
                request.PhoneNumber, request.Email, request.Notes);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Müşteri sil
    /// </summary>
    [HttpDelete("{customerId}")]
    public async Task<IActionResult> Delete(int businessId, int customerId)
    {
        try
        {
            await _customerService.DeleteAsync(businessId, customerId);
            return Ok(new { message = "Müşteri silindi" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}