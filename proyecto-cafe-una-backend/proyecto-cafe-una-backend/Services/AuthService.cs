using proyecto_cafe_una_backend.Models;
using System.Net;
using System.Net.Mail;

namespace proyecto_cafe_una_backend.Services;

public class AuthService(UsuariosService usuariosService, IWebHostEnvironment environment, IConfiguration configuration)
{
    private readonly JsonFileStore _resetStore = new("password_resets.json");
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(30);

    public async Task<Usuario> RegistrarAsync(RegisterRequest request)
    {
        var nombre = request.Nombre.Trim();
        var correo = request.Correo.Trim().ToLowerInvariant();
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(correo))
        {
            throw new InvalidOperationException("El correo es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            throw new InvalidOperationException("La contraseña debe tener al menos 6 caracteres.");
        }

        if (await usuariosService.ExisteCorreoAsync(correo))
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");
        }

        return await usuariosService.CrearAsync(new Usuario
        {
            Nombre = nombre,
            Correo = correo,
            PasswordHash = password,
            Estado = "activo",
            Roles = ["Usuario"]
        });
    }

    public async Task<ForgotPasswordResult> SolicitarRecuperacionAsync(ForgotPasswordRequest request)
    {
        var identifier = request.Identifier.Trim();
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return new ForgotPasswordResult { UsuarioEncontrado = false };
        }

        var usuario = await usuariosService.ObtenerPorNombreOCorreoAsync(identifier);
        if (usuario is null || !string.Equals(usuario.Estado, "activo", StringComparison.OrdinalIgnoreCase))
        {
            return new ForgotPasswordResult { UsuarioEncontrado = false };
        }

        var entries = await _resetStore.ReadAsync(() => new List<PasswordResetEntry>());
        var now = DateTime.UtcNow;

        entries.RemoveAll(entry =>
            entry.Usado ||
            entry.ExpiraEnUtc <= now ||
            entry.Correo.Equals(usuario.Correo, StringComparison.OrdinalIgnoreCase)
        );

        var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        entries.Add(new PasswordResetEntry
        {
            Token = token,
            Correo = usuario.Correo,
            ExpiraEnUtc = now.Add(_tokenLifetime),
            Usado = false
        });

        await _resetStore.WriteAsync(entries);

        var emailEnviado = await EnviarCorreoRecuperacionAsync(usuario.Correo, usuario.Nombre, token);
        return new ForgotPasswordResult
        {
            UsuarioEncontrado = true,
            DevToken = environment.IsDevelopment() ? token : null,
            EmailEnviado = emailEnviado
        };
    }

    public async Task<bool> RestablecerPasswordAsync(ResetPasswordRequest request)
    {
        var token = request.Token.Trim().ToUpperInvariant();
        var nuevaPassword = request.NuevaPassword;
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
        {
            return false;
        }

        var entries = await _resetStore.ReadAsync(() => new List<PasswordResetEntry>());
        var now = DateTime.UtcNow;
        var entry = entries.FirstOrDefault(e =>
            !e.Usado &&
            e.Token.Equals(token, StringComparison.OrdinalIgnoreCase) &&
            e.ExpiraEnUtc > now
        );

        if (entry is null)
        {
            return false;
        }

        var actualizado = await usuariosService.ActualizarPasswordPorCorreoAsync(entry.Correo, nuevaPassword);
        if (actualizado is null)
        {
            return false;
        }

        entry.Usado = true;
        await _resetStore.WriteAsync(entries);
        return true;
    }

    private async Task<bool> EnviarCorreoRecuperacionAsync(string destinatario, string nombre, string token)
    {
        var settings = configuration.GetSection("Smtp").Get<SmtpSettings>();
        if (settings is null || string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.FromEmail))
        {
            return false;
        }

        try
        {
            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(settings.Username))
            {
                client.Credentials = new NetworkCredential(settings.Username, settings.Password);
            }

            using var message = new MailMessage
            {
                From = new MailAddress(settings.FromEmail, settings.FromName),
                Subject = "Codigo de recuperacion de contrasena",
                Body = $"Hola {nombre},\n\nTu codigo de recuperacion es: {token}\n\nEste codigo vence en 30 minutos.\n\nSi no solicitaste este cambio, ignora este correo.",
                IsBodyHtml = false
            };

            message.To.Add(destinatario);
            await client.SendMailAsync(message);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
