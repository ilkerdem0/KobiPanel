using Microsoft.EntityFrameworkCore;
using KobiPanel.Core.Entities;
using KobiPanel.Infrastructure.Data;

namespace KobiPanel.Business.Services;

public class InvoiceService
{
    private readonly AppDbContext _context;

    public InvoiceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> CreateAsync(int businessId, int? customerId, DateTime? dueDate, decimal taxRate, string? notes, List<InvoiceItemInput> items)
    {
        var invoiceCount = await _context.Invoices.CountAsync(i => i.BusinessId == businessId);
        var invoiceNumber = $"KBP-{DateTime.UtcNow:yyyy}-{(invoiceCount + 1):D4}";

        var subTotal = items.Sum(i => i.Quantity * i.UnitPrice);
        var taxAmount = subTotal * taxRate / 100;
        var totalAmount = subTotal + taxAmount;

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = DateTime.UtcNow,
            DueDate = dueDate,
            SubTotal = subTotal,
            TaxRate = taxRate,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            Status = "Draft",
            Notes = notes,
            BusinessId = businessId,
            CustomerId = customerId,
            Items = items.Select(i => new InvoiceItem
            {
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.Quantity * i.UnitPrice
            }).ToList()
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return await MapToResponse(invoice);
    }

    public async Task<object> GetAllAsync(int businessId, int page, int pageSize, string? status)
    {
        var query = _context.Invoices
            .Where(i => i.BusinessId == businessId)
            .Include(i => i.Customer)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(i => i.Status == status);

        var totalCount = await query.CountAsync();
        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new
        {
            Items = invoices.Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.InvoiceDate,
                i.DueDate,
                i.TotalAmount,
                i.Status,
                CustomerName = i.Customer != null ? i.Customer.FullName : null
            }),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<object> GetByIdAsync(int businessId, int invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.BusinessId == businessId);

        if (invoice == null)
            throw new Exception("Fatura bulunamadı.");

        return await MapToResponse(invoice);
    }

    public async Task<object> UpdateStatusAsync(int businessId, int invoiceId, string status)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.BusinessId == businessId);

        if (invoice == null)
            throw new Exception("Fatura bulunamadı.");

        invoice.Status = status;
        await _context.SaveChangesAsync();

        return await MapToResponse(invoice);
    }

    public async Task DeleteAsync(int businessId, int invoiceId)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.BusinessId == businessId);

        if (invoice == null)
            throw new Exception("Fatura bulunamadı.");

        invoice.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    private async Task<object> MapToResponse(Invoice i)
    {
        if (i.Customer == null && i.CustomerId.HasValue)
            await _context.Entry(i).Reference(x => x.Customer).LoadAsync();
        if (i.Items == null || !i.Items.Any())
            await _context.Entry(i).Collection(x => x.Items).LoadAsync();

        return new
        {
            i.Id,
            i.InvoiceNumber,
            i.InvoiceDate,
            i.DueDate,
            i.SubTotal,
            i.TaxRate,
            i.TaxAmount,
            i.TotalAmount,
            i.Status,
            i.Notes,
            CustomerName = i.Customer?.FullName,
            Items = i.Items?.Where(item => !item.IsDeleted).Select(item => new
            {
                item.Id,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.TotalPrice
            })
        };
    }
}

public class InvoiceItemInput
{
    public string Description { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}