namespace proyecto_cafe_una_backend.Models;

public class SolicitudVoluntariado
{
    public long Id { get; set; }
    public string UserId { get; set; } = "anonimo";
    public string FechaSolicitud { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
    public string Estado { get; set; } = "Pendiente";
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? TipoVoluntariado { get; set; }
    public string? Identificacion { get; set; }
    public string? Institucion { get; set; }
    public string? Pais { get; set; }
    public string? Modalidad { get; set; }
    public int? CantidadParticipantes { get; set; }
    public string? Residencia { get; set; }
    public string? Horario { get; set; }
    public string? Dias { get; set; }
    public string? Area { get; set; }
    public string? Descripcion { get; set; }
    public string? Motivacion { get; set; }
}
