using System.Net;
using System.Net.Http.Json;

namespace KobiPanel.Tests;

public class ValidationAndErrorTests : IClassFixture<KobiPanelFactory>
{
    private readonly KobiPanelFactory _factory;

    public ValidationAndErrorTests(KobiPanelFactory factory) => _factory = factory;

    private static async Task<string?> MessageOf(HttpResponseMessage res) =>
        (await res.ReadJsonAsync()).GetProperty("message").GetString();

    private static object Transaction(decimal amount = 100, string type = "Income", bool isScheduled = false,
        DateTime? dueDate = null, string? paymentMethod = "Cash") => new
    {
        description = "Satış",
        amount,
        type,
        transactionDate = DateTime.UtcNow,
        paymentMethod,
        isScheduled,
        dueDate
    };

    [Fact]
    public async Task Olmayan_kayit_404_ve_mesaj_doner()
    {
        var user = await TestUser.CreateAsync(_factory);

        var res = await user.Client.GetAsync($"/api/businesses/{user.BusinessId}/Customers/999999");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal("Müşteri bulunamadı.", await MessageOf(res));
    }

    [Theory]
    [InlineData(0, "Income", "Tutar sıfırdan büyük olmalıdır.")]
    [InlineData(-5, "Income", "Tutar sıfırdan büyük olmalıdır.")]
    [InlineData(100, "Hediye", "İşlem türü Income veya Expense olmalıdır.")]
    public async Task Gecersiz_islem_400_ve_mesaj_doner(decimal amount, string type, string expected)
    {
        var user = await TestUser.CreateAsync(_factory);

        var res = await user.Client.PostAsJsonAsync($"/api/businesses/{user.BusinessId}/Transactions",
            Transaction(amount, type));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal(expected, await MessageOf(res));
    }

    [Fact]
    public async Task Vadeli_islem_vade_tarihi_olmadan_reddedilir()
    {
        var user = await TestUser.CreateAsync(_factory);

        var res = await user.Client.PostAsJsonAsync($"/api/businesses/{user.BusinessId}/Transactions",
            Transaction(isScheduled: true, dueDate: null));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Gecerli_islem_kabul_edilir()
    {
        var user = await TestUser.CreateAsync(_factory);

        var res = await user.Client.PostAsJsonAsync($"/api/businesses/{user.BusinessId}/Transactions",
            Transaction(isScheduled: true, dueDate: DateTime.UtcNow.AddDays(10)));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Kalemsiz_fatura_reddedilir()
    {
        var user = await TestUser.CreateAsync(_factory);

        var res = await user.Client.PostAsJsonAsync($"/api/businesses/{user.BusinessId}/Invoices",
            new { taxRate = 20, items = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("Fatura en az bir kalem içermelidir.", await MessageOf(res));
    }

    [Fact]
    public async Task Gecersiz_fatura_durumu_reddedilir()
    {
        var user = await TestUser.CreateAsync(_factory);
        var invoiceId = (await (await user.CreateInvoiceAsync()).ReadJsonAsync()).GetProperty("id").GetInt32();

        var res = await user.Client.PutAsJsonAsync($"/api/businesses/{user.BusinessId}/Invoices/{invoiceId}/status",
            new { status = "Bilinmiyor" });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Opsiyonel_alanlar_bos_string_gelirse_kabul_edilir()
    {
        // Frontend formları boş alanları "" olarak gönderiyor
        var user = await TestUser.CreateAsync(_factory);

        var customer = await user.Client.PostAsJsonAsync($"/api/businesses/{user.BusinessId}/Customers",
            new { firstName = "Ali", lastName = "Veli", phoneNumber = "", email = "", notes = "" });
        var business = await user.Client.PostAsJsonAsync("/api/Businesses",
            new { name = "İkinci", description = "", address = "", phoneNumber = "", email = "", taxNumber = "", businessType = "General" });

        Assert.Equal(HttpStatusCode.OK, customer.StatusCode);
        Assert.Equal(HttpStatusCode.OK, business.StatusCode);
    }

    [Fact]
    public async Task Gecersiz_email_reddedilir()
    {
        var user = await TestUser.CreateAsync(_factory);

        var res = await user.Client.PostAsJsonAsync($"/api/businesses/{user.BusinessId}/Customers",
            new { firstName = "Ali", lastName = "Veli", email = "email-degil" });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("Geçerli bir email adresi giriniz.", await MessageOf(res));
    }

    [Fact]
    public async Task Kisa_sifreyle_kayit_reddedilir()
    {
        var client = _factory.CreateClient();

        var res = await client.PostAsJsonAsync("/api/Auth/register",
            new { firstName = "A", lastName = "B", email = $"short-{Guid.NewGuid():N}@example.test", password = "123" });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("Şifre en az 8 karakter olmalıdır.", await MessageOf(res));
    }
}
