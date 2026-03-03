using Microsoft.EntityFrameworkCore;
using KobiPanel.Core.Entities;
using KobiPanel.Infrastructure.Data;

namespace KobiPanel.Business.Services;

public class TransactionService
{
    private readonly AppDbContext _context;

    public TransactionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> CreateAsync(int businessId, string description, decimal amount, string type, DateTime transactionDate, string? paymentMethod, int? categoryId, int? customerId)
    {
        var transaction = new Transaction
        {
            Description = description,
            Amount = amount,
            Type = type,
            TransactionDate = transactionDate,
            PaymentMethod = paymentMethod,
            CategoryId = categoryId,
            CustomerId = customerId,
            BusinessId = businessId
        };

        _context.Transactions.Add(transaction);

        // Müşterinin toplam harcamasını güncelle
        if (customerId.HasValue && type == "Income")
        {
            var customer = await _context.Customers.FindAsync(customerId.Value);
            if (customer != null)
                customer.TotalSpent += amount;
        }

        await _context.SaveChangesAsync();

        return await MapToResponse(transaction);
    }

    public async Task<object> GetAllAsync(int businessId, int page, int pageSize, string? type, DateTime? from, DateTime? to)
    {
        var query = _context.Transactions
            .Where(t => t.BusinessId == businessId)
            .Include(t => t.Category)
            .Include(t => t.Customer)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
            query = query.Where(t => t.Type == type);

        if (from.HasValue)
            query = query.Where(t => t.TransactionDate >= from.Value);

        if (to.HasValue)
            query = query.Where(t => t.TransactionDate <= to.Value);

        var totalCount = await query.CountAsync();
        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<object>();
        foreach (var t in transactions)
            items.Add(await MapToResponse(t));

        return new
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<object> GetSummaryAsync(int businessId, DateTime from, DateTime to)
    {
        var transactions = await _context.Transactions
            .Where(t => t.BusinessId == businessId && t.TransactionDate >= from && t.TransactionDate <= to)
            .ToListAsync();

        return new
        {
            TotalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount),
            TotalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount),
            NetProfit = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount) - transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount),
            TransactionCount = transactions.Count,
            PeriodStart = from,
            PeriodEnd = to
        };
    }

    public async Task DeleteAsync(int businessId, int transactionId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.BusinessId == businessId);

        if (transaction == null)
            throw new Exception("İşlem bulunamadı.");

        transaction.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    private async Task<object> MapToResponse(Transaction t)
    {
        if (t.Category == null && t.CategoryId.HasValue)
            await _context.Entry(t).Reference(x => x.Category).LoadAsync();
        if (t.Customer == null && t.CustomerId.HasValue)
            await _context.Entry(t).Reference(x => x.Customer).LoadAsync();

        return new
        {
            t.Id,
            t.Description,
            t.Amount,
            t.Type,
            t.TransactionDate,
            t.PaymentMethod,
            CategoryName = t.Category?.Name,
            CustomerName = t.Customer != null ? t.Customer.FullName : null,
            t.CreatedAt
        };
    }
}