using System.Net;
using System.Net.Http.Json;

namespace KobiPanel.Tests;

public class BusinessOwnershipTests : IClassFixture<KobiPanelFactory>
{
    private readonly KobiPanelFactory _factory;

    public BusinessOwnershipTests(KobiPanelFactory factory) => _factory = factory;

    public static TheoryData<string> BusinessScopedGetPaths => new()
    {
        "Customers",
        "Dashboard",
        "Invoices",
        "Transactions",
        "Transactions/summary?from=2026-01-01&to=2026-12-31",
        "Transactions/chart?days=30",
    };

    [Theory]
    [MemberData(nameof(BusinessScopedGetPaths))]
    public async Task Baska_kullanicinin_isletmesine_erisim_404_doner(string path)
    {
        var owner = await TestUser.CreateAsync(_factory);
        var attacker = await TestUser.CreateAsync(_factory);

        var res = await attacker.Client.GetAsync($"/api/businesses/{owner.BusinessId}/{path}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Theory]
    [MemberData(nameof(BusinessScopedGetPaths))]
    public async Task Kendi_isletmesine_erisim_200_doner(string path)
    {
        var owner = await TestUser.CreateAsync(_factory);

        var res = await owner.Client.GetAsync($"/api/businesses/{owner.BusinessId}/{path}");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Token_olmadan_401_doner()
    {
        var owner = await TestUser.CreateAsync(_factory);
        var anonymous = _factory.CreateClient();

        var res = await anonymous.GetAsync($"/api/businesses/{owner.BusinessId}/Customers");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Baska_kullanicinin_musterisi_silinemez()
    {
        var owner = await TestUser.CreateAsync(_factory);
        var attacker = await TestUser.CreateAsync(_factory);
        var customerId = await owner.CreateCustomerAsync();

        var res = await attacker.Client.DeleteAsync($"/api/businesses/{owner.BusinessId}/Customers/{customerId}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var stillThere = await owner.Client.GetAsync($"/api/businesses/{owner.BusinessId}/Customers/{customerId}");
        Assert.Equal(HttpStatusCode.OK, stillThere.StatusCode);
    }

    [Fact]
    public async Task Baska_isletmenin_musterisiyle_islem_olusturulamaz()
    {
        var owner = await TestUser.CreateAsync(_factory);
        var attacker = await TestUser.CreateAsync(_factory);
        var victimCustomerId = await owner.CreateCustomerAsync();

        var res = await attacker.Client.PostAsJsonAsync($"/api/businesses/{attacker.BusinessId}/Transactions", new
        {
            description = "x",
            amount = 999,
            type = "Income",
            transactionDate = DateTime.UtcNow,
            customerId = victimCustomerId,
            isScheduled = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var customer = await (await owner.Client.GetAsync(
            $"/api/businesses/{owner.BusinessId}/Customers/{victimCustomerId}")).ReadJsonAsync();
        Assert.Equal(0m, customer.GetProperty("totalSpent").GetDecimal());
    }

    [Fact]
    public async Task Baska_isletmenin_musterisiyle_fatura_olusturulamaz()
    {
        var owner = await TestUser.CreateAsync(_factory);
        var attacker = await TestUser.CreateAsync(_factory);
        var victimCustomerId = await owner.CreateCustomerAsync();

        var res = await attacker.CreateInvoiceAsync(victimCustomerId);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
