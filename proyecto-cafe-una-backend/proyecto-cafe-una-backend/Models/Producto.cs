namespace proyecto_cafe_una_backend.Models;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Peso { get; set; }
    public string? Imagen { get; set; }
    public int PrecioNormal { get; set; }
    public int PrecioConIva { get; set; }
    public int Stock { get; set; }
    public string Estado { get; set; } = "Habilitado";
}
