using Microsoft.EntityFrameworkCore;
using KobiPanel.Core.Entities;
using KobiPanel.Infrastructure.Data;

namespace KobiPanel.Business.Services;

public class CustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> CreateAsync(int businessId, string firstName, string lastName, string? phone, string? email, string? notes)
    {
        var customer = new Customer
        {
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phone,
            Email = email,
            Notes = notes,
            BusinessId = businessId
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return MapToResponse(customer);
    }

    public async Task<object> GetAllAsync(int businessId, int page, int pageSize, string? search)
    {
        var query = _context.Customers
            .Where(c => c.BusinessId == businessId);

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(search) ||
                c.LastName.ToLower().Contains(search) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new
        {
            Items = customers.Select(c => new
            {
                c.Id,
                FullName = c.FullName,
                c.PhoneNumber,
                c.TotalSpent,
                c.CreatedAt
            }),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<object> GetByIdAsync(int businessId, int customerId)
    {
        var customer = await _context.Customers
            .Include(c => c.Transactions.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == customerId && c.BusinessId == businessId);

        if (customer == null)
            throw new Exception("Müşteri bulunamadı.");

        return MapToDetailResponse(customer);
    }

    public async Task<object> UpdateAsync(int businessId, int customerId, string firstName, string lastName, string? phone, string? email, string? notes)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && c.BusinessId == businessId);

        if (customer == null)
            throw new Exception("Müşteri bulunamadı.");

        customer.FirstName = firstName;
        customer.LastName = lastName;
        customer.PhoneNumber = phone;
        customer.Email = email;
        customer.Notes = notes;

        await _context.SaveChangesAsync();
        return MapToResponse(customer);
    }

    public async Task DeleteAsync(int businessId, int customerId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && c.BusinessId == businessId);

        if (customer == null)
            throw new Exception("Müşteri bulunamadı.");

        customer.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    private object MapToResponse(Customer c)
    {
        return new
        {
            c.Id,
            c.FirstName,
            c.LastName,
            FullName = c.FullName,
            c.PhoneNumber,
            c.Email,
            c.Notes,
            c.TotalSpent,
            c.CreatedAt
        };
    }

    private object MapToDetailResponse(Customer c)
    {
        return new
        {
            c.Id,
            c.FirstName,
            c.LastName,
            FullName = c.FullName,
            c.PhoneNumber,
            c.Email,
            c.Notes,
            c.TotalSpent,
            TransactionCount = c.Transactions?.Count ?? 0,
            c.CreatedAt
        };
    }
}