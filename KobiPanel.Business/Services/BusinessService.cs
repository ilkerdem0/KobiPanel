using Microsoft.EntityFrameworkCore;
using KobiPanel.Core.Entities;
using KobiPanel.Infrastructure.Data;

namespace KobiPanel.Business.Services;

public class BusinessService
{
    private readonly AppDbContext _context;

    public BusinessService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> CreateAsync(int userId, string name, string? description, string? address, string? phone, string? email, string? taxNumber, string businessType)
    {
        var business = new Core.Entities.Business
        {
            Name = name,
            Description = description,
            Address = address,
            PhoneNumber = phone,
            Email = email,
            TaxNumber = taxNumber,
            BusinessType = businessType,
            OwnerId = userId
        };

        _context.Businesses.Add(business);
        await _context.SaveChangesAsync();

        return MapToResponse(business);
    }

    public async Task<List<object>> GetUserBusinessesAsync(int userId)
    {
        var businesses = await _context.Businesses
            .Where(b => b.OwnerId == userId)
            .Include(b => b.Customers)
            .ToListAsync();

        return businesses.Select(b => MapToResponse(b)).ToList();
    }

    public async Task<object> GetByIdAsync(int userId, int businessId)
    {
        var business = await _context.Businesses
            .Include(b => b.Customers)
            .FirstOrDefaultAsync(b => b.Id == businessId && b.OwnerId == userId);

        if (business == null)
            throw new Exception("İşletme bulunamadı.");

        return MapToResponse(business);
    }

    public async Task<object> UpdateAsync(int userId, int businessId, string name, string? description, string? address, string? phone, string? email, string? taxNumber, string businessType)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.OwnerId == userId);

        if (business == null)
            throw new Exception("İşletme bulunamadı.");

        business.Name = name;
        business.Description = description;
        business.Address = address;
        business.PhoneNumber = phone;
        business.Email = email;
        business.TaxNumber = taxNumber;
        business.BusinessType = businessType;

        await _context.SaveChangesAsync();
        return MapToResponse(business);
    }

    public async Task DeleteAsync(int userId, int businessId)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.OwnerId == userId);

        if (business == null)
            throw new Exception("İşletme bulunamadı.");

        business.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    private object MapToResponse(Core.Entities.Business b)
    {
        return new
        {
            b.Id,
            b.Name,
            b.Description,
            b.Address,
            b.PhoneNumber,
            b.Email,
            b.TaxNumber,
            b.BusinessType,
            CustomerCount = b.Customers?.Count(c => !c.IsDeleted) ?? 0,
            b.CreatedAt
        };
    }
}