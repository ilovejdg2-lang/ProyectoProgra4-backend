namespace proyecto_cafe_una_backend.Models;

public class RegisterRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Identifier { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NuevaPassword { get; set; } = string.Empty;
}

public class ForgotPasswordResult
{
    public bool UsuarioEncontrado { get; set; }
    public string? DevToken { get; set; }
}

public class PasswordResetEntry
{
    public string Token { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public DateTime ExpiraEnUtc { get; set; }
    public bool Usado { get; set; }
}
