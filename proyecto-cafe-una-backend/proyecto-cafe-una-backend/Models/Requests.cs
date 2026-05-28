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
    public int? ActorId { get; set; }
    public List<string> ActorRoles { get; set; } = [];
}

public class ActualizarUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public int? ActorId { get; set; }
    public List<string> ActorRoles { get; set; } = [];
}

public class ActualizarSeccionRequest
{
    public JsonObject Cambios { get; set; } = [];
}

public class ActorRequest
{
    public List<string> ActorRoles { get; set; } = [];
}

public class ProductoRequest : ActorRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Peso { get; set; }
    public string? Imagen { get; set; }
    public int PrecioNormal { get; set; }
    public int PrecioConIva { get; set; }
    public int Stock { get; set; }
    public string Estado { get; set; } = "Habilitado";
}
