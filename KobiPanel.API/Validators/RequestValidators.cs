using FluentValidation;
using KobiPanel.Business.Services;
using static KobiPanel.API.Controllers.AuthController;
using static KobiPanel.API.Controllers.BusinessesController;
using static KobiPanel.API.Controllers.CustomersController;
using static KobiPanel.API.Controllers.InvoicesController;
using static KobiPanel.API.Controllers.TransactionsController;

namespace KobiPanel.API.Validators;

// Uzunluk sınırları AppDbContext'teki HasMaxLength değerleriyle aynı tutulmalı.
// Frontend opsiyonel alanları boş string olarak gönderdiği için opsiyonel
// alan kuralları yalnızca değer doluysa çalışır.

internal static class Rules
{
    public static readonly string[] TransactionTypes = { "Income", "Expense" };
    public static readonly string[] PaymentMethods = { "Cash", "CreditCard", "BankTransfer" };
    public static readonly string[] InvoiceStatuses = { "Draft", "Sent", "Paid", "Cancelled" };

    public static IRuleBuilderOptions<T, string?> OptionalEmail<T>(this IRuleBuilder<T, string?> rule) =>
        rule.EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
            .MaximumLength(256).WithMessage("Email en fazla 256 karakter olabilir.");
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Ad zorunludur.")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Soyad zorunludur.")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email zorunludur.").OptionalEmail();
        RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre zorunludur.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email zorunludur.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre zorunludur.");
    }
}

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token zorunludur.");
    }
}

public class CreateBusinessRequestValidator : AbstractValidator<CreateBusinessRequest>
{
    public CreateBusinessRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("İşletme adı zorunludur.")
            .MaximumLength(200).WithMessage("İşletme adı en fazla 200 karakter olabilir.");
        RuleFor(x => x.BusinessType).NotEmpty().WithMessage("İşletme türü zorunludur.")
            .MaximumLength(100).WithMessage("İşletme türü en fazla 100 karakter olabilir.");
        RuleFor(x => x.TaxNumber).MaximumLength(50).WithMessage("Vergi numarası en fazla 50 karakter olabilir.");
        RuleFor(x => x.Email).OptionalEmail().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Ad zorunludur.")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Soyad zorunludur.")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.");
        RuleFor(x => x.Email).OptionalEmail().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama zorunludur.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Tutar sıfırdan büyük olmalıdır.");
        RuleFor(x => x.Type).Must(t => Rules.TransactionTypes.Contains(t))
            .WithMessage("İşlem türü Income veya Expense olmalıdır.");
        RuleFor(x => x.PaymentMethod).Must(m => Rules.PaymentMethods.Contains(m))
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod))
            .WithMessage("Ödeme yöntemi Cash, CreditCard veya BankTransfer olmalıdır.");
        RuleFor(x => x.DueDate).NotNull().When(x => x.IsScheduled)
            .WithMessage("Vadeli işlemler için vade tarihi zorunludur.");
    }
}

public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.TaxRate).InclusiveBetween(0, 100).WithMessage("KDV oranı 0 ile 100 arasında olmalıdır.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Fatura en az bir kalem içermelidir.");
        RuleForEach(x => x.Items).SetValidator(new InvoiceItemInputValidator());
    }
}

public class InvoiceItemInputValidator : AbstractValidator<InvoiceItemInput>
{
    public InvoiceItemInputValidator()
    {
        RuleFor(x => x.Description).NotEmpty().WithMessage("Kalem açıklaması zorunludur.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Miktar sıfırdan büyük olmalıdır.");
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Birim fiyat negatif olamaz.");
    }
}

public class UpdateStatusRequestValidator : AbstractValidator<UpdateStatusRequest>
{
    public UpdateStatusRequestValidator()
    {
        RuleFor(x => x.Status).Must(s => Rules.InvoiceStatuses.Contains(s))
            .WithMessage("Durum Draft, Sent, Paid veya Cancelled olmalıdır.");
    }
}
