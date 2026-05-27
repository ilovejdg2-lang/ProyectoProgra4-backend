using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var usuario = await authService.RegistrarAsync(request);
            return Ok(new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                correo = usuario.Correo,
                estado = usuario.Estado,
                roles = usuario.Roles
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await authService.SolicitarRecuperacionAsync(request);
        if (!result.UsuarioEncontrado)
        {
            return Ok(new
            {
                found = false,
                message = "No hay ningún usuario con ese correo o nombre de usuario.",
                devToken = (string?)null
            });
        }

        return Ok(new
        {
            found = true,
            message = result.EmailEnviado
                ? "Se envio el codigo de recuperacion al correo registrado."
                : "Se genero el codigo de recuperacion.",
            devToken = result.DevToken,
            emailSent = result.EmailEnviado
        });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await authService.RestablecerPasswordAsync(request);
        if (!success)
        {
            return BadRequest(new { message = "Token inválido/expirado o contraseña no válida." });
        }

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }
}
