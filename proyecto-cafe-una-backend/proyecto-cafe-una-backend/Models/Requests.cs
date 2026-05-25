using System.Text.Json.Nodes;

namespace proyecto_cafe_una_backend.Models;

public class AjusteStockItem
{
    public int Id { get; set; }
    public int Units { get; set; }
}

public class CambiarEstadoUsuarioRequest
{
    public string? Estado { get; set; }
}

public class ActualizarSeccionRequest
{
    public JsonObject Cambios { get; set; } = [];
}
