using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController(ProductosService productosService) : ControllerBase
{
    private static bool EsAdminOSuperAdmin(IEnumerable<string>? roles) =>
        roles?.Any(rol =>
            string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(rol, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
        ) == true;

    private static bool EsSuperAdmin(IEnumerable<string>? roles) =>
        roles?.Any(rol => string.Equals(rol, "SuperAdmin", StringComparison.OrdinalIgnoreCase)) == true;

    private static Producto MapProducto(ProductoRequest request) =>
        new()
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Peso = request.Peso,
            Imagen = request.Imagen,
            PrecioNormal = request.PrecioNormal,
            PrecioConIva = request.PrecioConIva,
            Stock = request.Stock,
            Estado = request.Estado
        };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> ObtenerProductos()
    {
        var productos = await productosService.ObtenerTodosAsync();
        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> ObtenerProductoPorId(int id)
    {
        var producto = await productosService.ObtenerPorIdAsync(id);
        if (producto is null)
        {
            return NotFound();
        }

        return Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> CrearProducto([FromBody] ProductoRequest nuevoProducto)
    {
        if (!EsAdminOSuperAdmin(nuevoProducto.ActorRoles))
        {
            return BadRequest(new { message = "Solo Admin o SuperAdmin pueden crear productos." });
        }

        var creado = await productosService.CrearAsync(MapProducto(nuevoProducto));
        return CreatedAtAction(nameof(ObtenerProductoPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Producto>> ActualizarProducto(int id, [FromBody] ProductoRequest cambios)
    {
        if (!EsAdminOSuperAdmin(cambios.ActorRoles))
        {
            return BadRequest(new { message = "Solo Admin o SuperAdmin pueden modificar productos." });
        }

        var productoActual = await productosService.ObtenerPorIdAsync(id);
        if (productoActual is null)
        {
            return NotFound();
        }

        var estadoSolicitado = string.IsNullOrWhiteSpace(cambios.Estado) ? productoActual.Estado : cambios.Estado;
        var cambiaEstado = !string.Equals(productoActual.Estado, estadoSolicitado, StringComparison.OrdinalIgnoreCase);
        if (cambiaEstado && !EsSuperAdmin(cambios.ActorRoles))
        {
            return BadRequest(new { message = "Solo SuperAdmin puede habilitar o inhabilitar productos." });
        }

        var actualizado = await productosService.ActualizarAsync(id, MapProducto(cambios));
        if (actualizado is null)
        {
            return NotFound();
        }

        return Ok(actualizado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarProducto(int id, [FromBody] ActorRequest? request)
    {
        if (!EsSuperAdmin(request?.ActorRoles))
        {
            return BadRequest(new { message = "Solo SuperAdmin puede eliminar productos." });
        }

        var eliminado = await productosService.EliminarAsync(id);
        if (!eliminado)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("ajustar-stock")]
    public async Task<ActionResult<IEnumerable<Producto>>> AjustarStock([FromBody] List<AjusteStockItem> carritoItems)
    {
        try
        {
            var actualizados = await productosService.AjustarStockAsync(carritoItems);
            return Ok(actualizados);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                code = "STOCK_INSUFICIENTE",
                message = ex.Message
            });
        }
    }
}
