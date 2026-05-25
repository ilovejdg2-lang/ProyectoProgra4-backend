using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/voluntariado/solicitudes")]
public class VoluntariadoController(VoluntariadoService voluntariadoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitudVoluntariado>>> ObtenerSolicitudes()
    {
        var solicitudes = await voluntariadoService.ObtenerSolicitudesAsync();
        return Ok(solicitudes);
    }

    [HttpGet("usuario/{userId}")]
    public async Task<ActionResult<IEnumerable<SolicitudVoluntariado>>> ObtenerSolicitudesDeUsuario(string userId)
    {
        var solicitudes = await voluntariadoService.ObtenerSolicitudesDeUsuarioAsync(userId);
        return Ok(solicitudes);
    }

    [HttpPost]
    public async Task<ActionResult<SolicitudVoluntariado>> CrearSolicitud([FromBody] SolicitudVoluntariado nuevaSolicitud)
    {
        var creada = await voluntariadoService.CrearSolicitudAsync(nuevaSolicitud);
        return CreatedAtAction(nameof(ObtenerSolicitudes), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<SolicitudVoluntariado>> ActualizarSolicitud(long id, [FromBody] SolicitudVoluntariado cambios)
    {
        var actualizada = await voluntariadoService.ActualizarSolicitudAsync(id, cambios);
        if (actualizada is null)
        {
            return NotFound();
        }

        return Ok(actualizada);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> EliminarSolicitud(long id)
    {
        var deleted = await voluntariadoService.EliminarSolicitudAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
