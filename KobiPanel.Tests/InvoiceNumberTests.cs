namespace KobiPanel.Tests;

public class InvoiceNumberTests : IClassFixture<KobiPanelFactory>
{
    private readonly KobiPanelFactory _factory;
    private static readonly string Prefix = $"KBP-{DateTime.UtcNow:yyyy}-";

    public InvoiceNumberTests(KobiPanelFactory factory) => _factory = factory;

    private static async Task<(int Id, string Number)> CreateAsync(TestUser user)
    {
        var res = await user.CreateInvoiceAsync();
        res.EnsureSuccessStatusCode();
        var json = await res.ReadJsonAsync();
        return (json.GetProperty("id").GetInt32(), json.GetProperty("invoiceNumber").GetString()!);
    }

    [Fact]
    public async Task Numaralar_sirayla_artar()
    {
        var user = await TestUser.CreateAsync(_factory);

        var first = await CreateAsync(user);
        var second = await CreateAsync(user);

        Assert.Equal($"{Prefix}0001", first.Number);
        Assert.Equal($"{Prefix}0002", second.Number);
    }

    [Fact]
    public async Task Silinen_faturanin_numarasi_tekrar_kullanilmaz()
    {
        var user = await TestUser.CreateAsync(_factory);
        await CreateAsync(user);
        var second = await CreateAsync(user);

        var delete = await user.Client.DeleteAsync($"/api/businesses/{user.BusinessId}/Invoices/{second.Id}");
        delete.EnsureSuccessStatusCode();
        var third = await CreateAsync(user);

        Assert.Equal($"{Prefix}0003", third.Number);
    }

    [Fact]
    public async Task Her_isletmenin_numarasi_ayri_baslar()
    {
        var a = await TestUser.CreateAsync(_factory);
        var b = await TestUser.CreateAsync(_factory);

        await CreateAsync(a);
        await CreateAsync(a);
        var firstOfB = await CreateAsync(b);

        Assert.Equal($"{Prefix}0001", firstOfB.Number);
    }
}
