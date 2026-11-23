namespace KobiPanel.Core.Exceptions;

/// <summary>
/// Kullanıcıya mesajıyla birlikte gösterilebilecek, beklenen iş hataları.
/// API katmanındaki global handler bunları StatusCode + { message } olarak döner;
/// bunun dışındaki tüm exception'lar 500 ve genel bir mesajla döner.
/// </summary>
public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }

    protected AppException(string message) : base(message) { }
}

/// <summary>404 — kayıt yok veya kullanıcıya ait değil.</summary>
public class NotFoundException : AppException
{
    public override int StatusCode => 404;
    public NotFoundException(string message) : base(message) { }
}

/// <summary>400 — istek geçerli biçimde ama iş kuralına aykırı.</summary>
public class BadRequestException : AppException
{
    public override int StatusCode => 400;
    public BadRequestException(string message) : base(message) { }
}

/// <summary>409 — kayıt zaten var.</summary>
public class ConflictException : AppException
{
    public override int StatusCode => 409;
    public ConflictException(string message) : base(message) { }
}

/// <summary>401 — hatalı kimlik bilgisi veya geçersiz refresh token.</summary>
public class UnauthorizedException : AppException
{
    public override int StatusCode => 401;
    public UnauthorizedException(string message) : base(message) { }
}
