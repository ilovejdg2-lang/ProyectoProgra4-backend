using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class VoluntariadoService
{
    private readonly JsonFileStore _store = new("voluntariado.json");

    public async Task<List<SolicitudVoluntariado>> ObtenerSolicitudesAsync()
    {
        return await _store.ReadAsync(() => new List<SolicitudVoluntariado>());
    }

    public async Task<List<SolicitudVoluntariado>> ObtenerSolicitudesDeUsuarioAsync(string userId)
    {
        var solicitudes = await ObtenerSolicitudesAsync();
        return solicitudes.Where(s => s.UserId == userId).ToList();
    }

    public async Task<SolicitudVoluntariado> CrearSolicitudAsync(SolicitudVoluntariado nuevaSolicitud)
    {
        var solicitudes = await ObtenerSolicitudesAsync();
        var solicitudCompleta = new SolicitudVoluntariado
        {
            Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            UserId = string.IsNullOrWhiteSpace(nuevaSolicitud.UserId) ? "anonimo" : nuevaSolicitud.UserId,
            FechaSolicitud = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            Estado = "Pendiente",
            Nombre = nuevaSolicitud.Nombre,
            Email = nuevaSolicitud.Email,
            Telefono = nuevaSolicitud.Telefono,
            TipoVoluntariado = nuevaSolicitud.TipoVoluntariado,
            Identificacion = nuevaSolicitud.Identificacion,
            Institucion = nuevaSolicitud.Institucion,
            Pais = nuevaSolicitud.Pais,
            Modalidad = nuevaSolicitud.Modalidad,
            CantidadParticipantes = nuevaSolicitud.CantidadParticipantes,
            Residencia = nuevaSolicitud.Residencia,
            Horario = nuevaSolicitud.Horario,
            Dias = nuevaSolicitud.Dias,
            Area = nuevaSolicitud.Area,
            Descripcion = nuevaSolicitud.Descripcion,
            Motivacion = nuevaSolicitud.Motivacion
        };

        solicitudes.Add(solicitudCompleta);
        await _store.WriteAsync(solicitudes);
        return solicitudCompleta;
    }

    public async Task<SolicitudVoluntariado?> ActualizarSolicitudAsync(long id, SolicitudVoluntariado cambios)
    {
        var solicitudes = await ObtenerSolicitudesAsync();
        var index = solicitudes.FindIndex(s => s.Id == id);
        if (index < 0)
        {
            return null;
        }

        var actual = solicitudes[index];
        var actualizada = new SolicitudVoluntariado
        {
            Id = id,
            UserId = string.IsNullOrWhiteSpace(cambios.UserId) ? actual.UserId : cambios.UserId,
            FechaSolicitud = string.IsNullOrWhiteSpace(cambios.FechaSolicitud) ? actual.FechaSolicitud : cambios.FechaSolicitud,
            Estado = string.IsNullOrWhiteSpace(cambios.Estado) ? actual.Estado : cambios.Estado,
            Nombre = cambios.Nombre ?? actual.Nombre,
            Email = cambios.Email ?? actual.Email,
            Telefono = cambios.Telefono ?? actual.Telefono,
            TipoVoluntariado = cambios.TipoVoluntariado ?? actual.TipoVoluntariado,
            Identificacion = cambios.Identificacion ?? actual.Identificacion,
            Institucion = cambios.Institucion ?? actual.Institucion,
            Pais = cambios.Pais ?? actual.Pais,
            Modalidad = cambios.Modalidad ?? actual.Modalidad,
            CantidadParticipantes = cambios.CantidadParticipantes ?? actual.CantidadParticipantes,
            Residencia = cambios.Residencia ?? actual.Residencia,
            Horario = cambios.Horario ?? actual.Horario,
            Dias = cambios.Dias ?? actual.Dias,
            Area = cambios.Area ?? actual.Area,
            Descripcion = cambios.Descripcion ?? actual.Descripcion,
            Motivacion = cambios.Motivacion ?? actual.Motivacion
        };

        solicitudes[index] = actualizada;
        await _store.WriteAsync(solicitudes);
        return actualizada;
    }

    public async Task<bool> EliminarSolicitudAsync(long id)
    {
        var solicitudes = await ObtenerSolicitudesAsync();
        var filtradas = solicitudes.Where(s => s.Id != id).ToList();
        if (filtradas.Count == solicitudes.Count)
        {
            return false;
        }

        await _store.WriteAsync(filtradas);
        return true;
    }
}
