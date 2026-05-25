using Microsoft.AspNetCore.Mvc;
using proyecto_cafe_una_backend.Models;
using proyecto_cafe_una_backend.Services;

namespace proyecto_cafe_una_backend.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController(ProductosService productosService) : ControllerBase
{
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
    public async Task<ActionResult<Producto>> CrearProducto([FromBody] Producto nuevoProducto)
    {
        var creado = await productosService.CrearAsync(nuevoProducto);
        return CreatedAtAction(nameof(ObtenerProductoPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Producto>> ActualizarProducto(int id, [FromBody] Producto cambios)
    {
        var actualizado = await productosService.ActualizarAsync(id, cambios);
        if (actualizado is null)
        {
            return NotFound();
        }

        return Ok(actualizado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarProducto(int id)
    {
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
