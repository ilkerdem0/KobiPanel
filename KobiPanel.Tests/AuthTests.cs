using System.Net;
using System.Net.Http.Json;

namespace KobiPanel.Tests;

public class AuthTests : IClassFixture<KobiPanelFactory>
{
    private readonly KobiPanelFactory _factory;

    public AuthTests(KobiPanelFactory factory) => _factory = factory;

    private static object NewUser(string email) =>
        new { firstName = "Test", lastName = "User", email, password = "Test1234!" };

    private static string NewEmail() => $"auth-{Guid.NewGuid():N}@example.test";

    [Fact]
    public async Task Ayni_email_ile_ikinci_kayit_409_doner()
    {
        var client = _factory.CreateClient();
        var email = NewEmail();
        (await client.PostAsJsonAsync("/api/Auth/register", NewUser(email))).EnsureSuccessStatusCode();

        var res = await client.PostAsJsonAsync("/api/Auth/register", NewUser(email));

        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
        Assert.Equal("Bu email adresi zaten kayıtlı.", (await res.ReadJsonAsync()).GetProperty("message").GetString());
    }

    [Fact]
    public async Task Hatali_sifre_401_ve_mesaj_doner()
    {
        var client = _factory.CreateClient();
        var email = NewEmail();
        (await client.PostAsJsonAsync("/api/Auth/register", NewUser(email))).EnsureSuccessStatusCode();

        var res = await client.PostAsJsonAsync("/api/Auth/login", new { email, password = "yanlis-sifre" });

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.Equal("Email veya şifre hatalı.", (await res.ReadJsonAsync()).GetProperty("message").GetString());
    }

    [Fact]
    public async Task Refresh_yeni_token_verir_ve_eski_refresh_token_tekrar_kullanilamaz()
    {
        var client = _factory.CreateClient();
        var register = await (await client.PostAsJsonAsync("/api/Auth/register", NewUser(NewEmail()))).ReadJsonAsync();
        var oldRefresh = register.GetProperty("refreshToken").GetString();

        var refreshed = await client.PostAsJsonAsync("/api/Auth/refresh", new { refreshToken = oldRefresh });
        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);
        var body = await refreshed.ReadJsonAsync();
        Assert.False(string.IsNullOrEmpty(body.GetProperty("accessToken").GetString()));
        Assert.NotEqual(oldRefresh, body.GetProperty("refreshToken").GetString());

        var reused = await client.PostAsJsonAsync("/api/Auth/refresh", new { refreshToken = oldRefresh });
        Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);
    }

    [Fact]
    public async Task Logout_access_token_olmadan_refresh_tokeni_iptal_eder()
    {
        var client = _factory.CreateClient();
        var register = await (await client.PostAsJsonAsync("/api/Auth/register", NewUser(NewEmail()))).ReadJsonAsync();
        var refreshToken = register.GetProperty("refreshToken").GetString();

        var logout = await client.PostAsJsonAsync("/api/Auth/logout", new { refreshToken });
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        var refresh = await client.PostAsJsonAsync("/api/Auth/refresh", new { refreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }
}
