using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(UsuariosService usuariosService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> ObtenerUsuarios()
    {
        var usuarios = await usuariosService.ObtenerTodosAsync();
        return Ok(usuarios);
    }

    [HttpGet("activos")]
    public async Task<ActionResult<IEnumerable<Usuario>>> ObtenerUsuariosActivos()
    {
        var usuarios = await usuariosService.ObtenerActivosAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Usuario>> ObtenerUsuarioPorId(int id)
    {
        var usuario = await usuariosService.ObtenerPorIdAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<Usuario>> CrearUsuario([FromBody] Usuario nuevoUsuario)
    {
        var creado = await usuariosService.CrearAsync(nuevoUsuario);
        return CreatedAtAction(nameof(ObtenerUsuarioPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Usuario>> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioRequest cambios)
    {
        var payload = new Usuario
        {
            Nombre = cambios.Nombre,
            Correo = cambios.Correo,
            PasswordHash = cambios.PasswordHash,
            Estado = cambios.Estado,
            Roles = cambios.Roles
        };

        var actualizado = await usuariosService.ActualizarConActorAsync(id, payload, cambios.ActorId);
        if (actualizado is null)
        {
            return NotFound();
        }

        return Ok(actualizado);
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<ActionResult<Usuario>> ToggleEstadoUsuario(int id, [FromBody] CambiarEstadoUsuarioRequest? request)
    {
        try
        {
            var actualizado = await usuariosService.ToggleEstadoAsync(id, request?.Estado, request?.ActorId, request?.ActorRoles);
            if (actualizado is null)
            {
                return NotFound();
            }

            return Ok(actualizado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
