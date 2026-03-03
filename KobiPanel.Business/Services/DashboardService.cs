using Microsoft.EntityFrameworkCore;
using KobiPanel.Infrastructure.Data;

namespace KobiPanel.Business.Services;

public class DashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> GetSummaryAsync(int businessId)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var last30Days = today.AddDays(-30);

        var transactions = await _context.Transactions
            .Where(t => t.BusinessId == businessId && t.TransactionDate >= last30Days)
            .ToListAsync();

        var todayTransactions = transactions.Where(t => t.TransactionDate.Date == today);
        var monthTransactions = transactions.Where(t => t.TransactionDate >= monthStart);

        // Son 30 gün günlük gelir-gider
        var dailyRevenue = Enumerable.Range(0, 30)
            .Select(i => last30Days.AddDays(i))
            .Select(date => new
            {
                Date = date,
                Income = transactions.Where(t => t.TransactionDate.Date == date && t.Type == "Income").Sum(t => t.Amount),
                Expense = transactions.Where(t => t.TransactionDate.Date == date && t.Type == "Expense").Sum(t => t.Amount)
            }).ToList();

        // Kategori bazlı gider dağılımı
        var expenseByCategory = await _context.Transactions
            .Where(t => t.BusinessId == businessId && t.Type == "Expense" && t.TransactionDate >= monthStart)
            .Include(t => t.Category)
            .GroupBy(t => t.Category != null ? t.Category.Name : "Kategorisiz")
            .Select(g => new
            {
                CategoryName = g.Key,
                Amount = g.Sum(t => t.Amount)
            })
            .ToListAsync();

        // Yaklaşan randevular
        var upcomingAppointments = await _context.Appointments
            .Where(a => a.BusinessId == businessId && a.StartTime >= DateTime.UtcNow && a.Status == "Scheduled")
            .Include(a => a.Customer)
            .OrderBy(a => a.StartTime)
            .Take(5)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.StartTime,
                CustomerName = a.Customer != null ? a.Customer.FirstName + " " + a.Customer.LastName : null
            })
            .ToListAsync();

        var totalCustomers = await _context.Customers.CountAsync(c => c.BusinessId == businessId);
        var todayAppointments = await _context.Appointments.CountAsync(a => a.BusinessId == businessId && a.StartTime.Date == today);
        var pendingInvoices = await _context.Invoices.CountAsync(i => i.BusinessId == businessId && (i.Status == "Draft" || i.Status == "Sent"));

        return new
        {
            TodayIncome = todayTransactions.Where(t => t.Type == "Income").Sum(t => t.Amount),
            TodayExpense = todayTransactions.Where(t => t.Type == "Expense").Sum(t => t.Amount),
            MonthlyIncome = monthTransactions.Where(t => t.Type == "Income").Sum(t => t.Amount),
            MonthlyExpense = monthTransactions.Where(t => t.Type == "Expense").Sum(t => t.Amount),
            MonthlyNetProfit = monthTransactions.Where(t => t.Type == "Income").Sum(t => t.Amount) - monthTransactions.Where(t => t.Type == "Expense").Sum(t => t.Amount),
            TotalCustomers = totalCustomers,
            TodayAppointments = todayAppointments,
            PendingInvoices = pendingInvoices,
            Last30DaysRevenue = dailyRevenue,
            ExpenseByCategory = expenseByCategory,
            UpcomingAppointments = upcomingAppointments
        };
    }
}