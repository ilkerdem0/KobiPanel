using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KobiPanel.Business.Services;

namespace KobiPanel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessesController : ControllerBase
{
    private readonly BusinessService _businessService;

    public BusinessesController(BusinessService businessService)
    {
        _businessService = businessService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public record CreateBusinessRequest(
        string Name, string? Description, string? Address,
        string? PhoneNumber, string? Email, string? TaxNumber,
        string BusinessType
    );

    /// <summary>
    /// Yeni işletme oluştur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusinessRequest request)
    {
        try
        {
            var result = await _businessService.CreateAsync(
                GetUserId(), request.Name, request.Description,
                request.Address, request.PhoneNumber, request.Email,
                request.TaxNumber, request.BusinessType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcının tüm işletmelerini getir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _businessService.GetUserBusinessesAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// İşletme detayı
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _businessService.GetByIdAsync(GetUserId(), id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// İşletme güncelle
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateBusinessRequest request)
    {
        try
        {
            var result = await _businessService.UpdateAsync(
                GetUserId(), id, request.Name, request.Description,
                request.Address, request.PhoneNumber, request.Email,
                request.TaxNumber, request.BusinessType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// İşletme sil
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _businessService.DeleteAsync(GetUserId(), id);
            return Ok(new { message = "İşletme silindi" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}