using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/informacion")]
public class InformacionController(InformacionService informacionService) : ControllerBase
{
    private static bool EsSuperAdmin(IEnumerable<string>? roles) =>
        roles?.Any(rol => string.Equals(rol, "SuperAdmin", StringComparison.OrdinalIgnoreCase)) == true;

    [HttpGet]
    public async Task<ActionResult<JsonObject>> ObtenerInformacion()
    {
        var info = await informacionService.ObtenerInformacionAsync();
        return Ok(info);
    }

    [HttpGet("{seccion}")]
    public async Task<ActionResult<JsonNode>> ObtenerSeccion(string seccion)
    {
        var section = await informacionService.ObtenerSeccionAsync(seccion);
        if (section is null)
        {
            return NotFound();
        }

        return Ok(section);
    }

    [HttpPut]
    public async Task<ActionResult<JsonObject>> ActualizarInformacion([FromBody] JsonObject nuevaInformacion)
    {
        var info = await informacionService.ActualizarInformacionAsync(nuevaInformacion);
        return Ok(info);
    }

    [HttpPatch("{seccion}")]
    public async Task<ActionResult<JsonObject>> ActualizarSeccion(string seccion, [FromBody] JsonObject cambios)
    {
        var section = await informacionService.ActualizarSeccionAsync(seccion, cambios);
        return Ok(section);
    }

    [HttpPost("galeria")]
    public async Task<ActionResult<JsonObject>> AgregarGaleriaItem([FromBody] JsonObject item)
    {
        var nuevo = await informacionService.AgregarGaleriaItemAsync(item);
        return Ok(nuevo);
    }

    [HttpPut("galeria/{id:long}")]
    public async Task<ActionResult<JsonObject>> ActualizarGaleriaItem(long id, [FromBody] JsonObject cambios)
    {
        var actualizado = await informacionService.ActualizarGaleriaItemAsync(id, cambios);
        if (actualizado is null)
        {
            return NotFound();
        }

        return Ok(actualizado);
    }

    [HttpDelete("galeria/{id:long}")]
    public async Task<IActionResult> EliminarGaleriaItem(long id, [FromBody] ActorRequest? request)
    {
        if (!EsSuperAdmin(request?.ActorRoles))
        {
            return BadRequest(new { message = "Solo SuperAdmin puede eliminar imagenes." });
        }

        var deleted = await informacionService.EliminarGaleriaItemAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("hero")]
    public async Task<ActionResult<JsonNode>> ObtenerHero()
    {
        var hero = await informacionService.ObtenerSeccionAsync("hero");
        if (hero is null)
        {
            return NotFound();
        }

        return Ok(hero);
    }
}
