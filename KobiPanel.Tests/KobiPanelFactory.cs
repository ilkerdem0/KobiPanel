using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KobiPanel.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KobiPanel.Tests;

/// <summary>
/// API'yi SQL Server yerine izole bir InMemory veritabanıyla ayağa kaldırır.
/// </summary>
public class KobiPanelFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"KobiPanelTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // Program.cs secret'ı builder aşamasında okuduğu için UseSetting ile verilmeli
        builder.UseSetting("JwtSettings:Secret", "test-secret-key-that-is-at-least-32-bytes-long");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }
}

internal static class ServiceCollectionExtensions
{
    public static void RemoveAll<T>(this IServiceCollection services)
    {
        foreach (var d in services.Where(d => d.ServiceType == typeof(T)).ToList())
            services.Remove(d);
    }
}

/// <summary>
/// Kendi token'ı ve işletmesi olan, kayıtlı bir test kullanıcısı.
/// </summary>
public sealed class TestUser
{
    public required HttpClient Client { get; init; }
    public required int BusinessId { get; init; }

    public static async Task<TestUser> CreateAsync(KobiPanelFactory factory)
    {
        var client = factory.CreateClient();

        var register = await client.PostAsJsonAsync("/api/Auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email = $"test-{Guid.NewGuid():N}@example.test",
            password = "Test1234!"
        });
        register.EnsureSuccessStatusCode();
        var token = (await register.ReadJsonAsync()).GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var business = await client.PostAsJsonAsync("/api/Businesses", new { name = "Test İşletme", businessType = "Test" });
        business.EnsureSuccessStatusCode();
        var businessId = (await business.ReadJsonAsync()).GetProperty("id").GetInt32();

        return new TestUser { Client = client, BusinessId = businessId };
    }

    public async Task<int> CreateCustomerAsync()
    {
        var res = await Client.PostAsJsonAsync($"/api/businesses/{BusinessId}/Customers",
            new { firstName = "Müşteri", lastName = "Test" });
        res.EnsureSuccessStatusCode();
        return (await res.ReadJsonAsync()).GetProperty("id").GetInt32();
    }

    public Task<HttpResponseMessage> CreateInvoiceAsync(int? customerId = null) =>
        Client.PostAsJsonAsync($"/api/businesses/{BusinessId}/Invoices", new
        {
            customerId,
            taxRate = 20,
            items = new[] { new { description = "Hizmet", quantity = 1, unitPrice = 100 } }
        });
}

internal static class HttpExtensions
{
    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();
}
