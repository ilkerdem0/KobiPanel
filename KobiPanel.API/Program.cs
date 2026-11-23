using FluentValidation;
using FluentValidation.AspNetCore;
using KobiPanel.API.Infrastructure;
using KobiPanel.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== VERITABANI =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== JWT AUTHENTICATION =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// Secret repoda tutulmaz: geliştirmede `dotnet user-secrets`, production'da
// JwtSettings__Secret environment variable'ı ile verilir.
var jwtSecret = jwtSettings["Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException(
        "JwtSettings:Secret tanımlı değil veya 32 byte'tan kısa. " +
        "Geliştirme için: dotnet user-secrets set \"JwtSettings:Secret\" \"<en az 32 karakter>\" --project KobiPanel.API");
var secretKey = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

// ===== CORS (React frontend icin) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddScoped<KobiPanel.Business.Services.AuthService>();
builder.Services.AddScoped<KobiPanel.Business.Services.BusinessService>();
builder.Services.AddScoped<KobiPanel.Business.Services.CustomerService>();
builder.Services.AddScoped<KobiPanel.Business.Services.TransactionService>();
builder.Services.AddScoped<KobiPanel.Business.Services.DashboardService>();
builder.Services.AddScoped<KobiPanel.Business.Services.InvoiceService>();
// ===== CONTROLLERS & SWAGGER =====
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Doğrulama hataları da diğer hatalar gibi { message } döner (frontend bunu okur);
        // alan bazlı detaylar ayrıca "errors" altında verilir.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
            var message = errors.Values.SelectMany(x => x).FirstOrDefault() ?? "Geçersiz istek.";
            return new BadRequestObjectResult(new { message, errors });
        };
    });

// ===== DOĞRULAMA & HATA YÖNETİMİ =====
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KobiPanel API",
        Version = "v1",
        Description = "Küçük İşletme Yönetim Sistemi API"
    });

    // Swagger'da JWT token girebilmek icin
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token giriniz"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ===== MIDDLEWARE =====
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Entegrasyon testlerinde WebApplicationFactory<Program> için gerekli
public partial class Program { }