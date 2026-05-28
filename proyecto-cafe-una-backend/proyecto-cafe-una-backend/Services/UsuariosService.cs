using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class UsuariosService
{
    private readonly JsonFileStore _store = new("usuarios.json");
    private const string EstadoActivo = "activo";
    private const string EstadoInactivo = "inactivo";
    private const string RolSuperAdmin = "SuperAdmin";

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        return await _store.ReadAsync(DefaultUsuarios);
    }

    public async Task<List<Usuario>> ObtenerActivosAsync()
    {
        var usuarios = await ObtenerTodosAsync();
        return usuarios.Where(u => EsActivo(u.Estado)).ToList();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        var usuarios = await ObtenerTodosAsync();
        return usuarios.FirstOrDefault(u => u.Id == id);
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        var usuarios = await ObtenerTodosAsync();
        return usuarios.FirstOrDefault(u => u.Correo.Equals(correo.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Usuario?> ObtenerPorNombreOCorreoAsync(string identifier)
    {
        var normalized = identifier.Trim().ToLowerInvariant();
        var usuarios = await ObtenerTodosAsync();
        return usuarios.FirstOrDefault(u =>
            u.Correo.ToLowerInvariant() == normalized ||
            u.Nombre.ToLowerInvariant() == normalized
        );
    }

    public async Task<bool> ExisteCorreoAsync(string correo)
    {
        var usuario = await ObtenerPorCorreoAsync(correo);
        return usuario is not null;
    }

    public async Task<Usuario> CrearAsync(Usuario nuevoUsuario)
    {
        var usuarios = await ObtenerTodosAsync();
        var nextId = usuarios.Count == 0 ? 1 : usuarios.Max(u => u.Id) + 1;

        var usuarioCompleto = new Usuario
        {
            Id = nextId,
            Nombre = nuevoUsuario.Nombre,
            Correo = nuevoUsuario.Correo.ToLowerInvariant(),
            PasswordHash = nuevoUsuario.PasswordHash,
            Estado = "activo",
            Roles = nuevoUsuario.Roles.Count == 0 ? ["Usuario"] : nuevoUsuario.Roles
        };

        usuarios.Add(usuarioCompleto);
        await _store.WriteAsync(usuarios);
        return usuarioCompleto;
    }

    public async Task<Usuario?> ActualizarAsync(int id, Usuario cambios)
    {
        var usuarios = await ObtenerTodosAsync();
        var index = usuarios.FindIndex(u => u.Id == id);
        if (index < 0)
        {
            return null;
        }

        var actual = usuarios[index];
        var actualizado = new Usuario
        {
            Id = id,
            Nombre = string.IsNullOrWhiteSpace(cambios.Nombre) ? actual.Nombre : cambios.Nombre,
            Correo = string.IsNullOrWhiteSpace(cambios.Correo) ? actual.Correo : cambios.Correo.ToLowerInvariant(),
            PasswordHash = string.IsNullOrWhiteSpace(cambios.PasswordHash) ? actual.PasswordHash : cambios.PasswordHash,
            Estado = string.IsNullOrWhiteSpace(cambios.Estado) ? actual.Estado : cambios.Estado,
            Roles = cambios.Roles.Count == 0 ? actual.Roles : cambios.Roles
        };

        usuarios[index] = actualizado;
        await _store.WriteAsync(usuarios);
        return actualizado;
    }

    public async Task<Usuario?> ActualizarConActorAsync(int id, Usuario cambios, int? actorId, IEnumerable<string>? actorRoles = null)
    {
        var usuarios = await ObtenerTodosAsync();
        var index = usuarios.FindIndex(u => u.Id == id);
        if (index < 0)
        {
            return null;
        }

        var actual = usuarios[index];
        var puedeCambiarPassword = actorId.HasValue && actorId.Value == id;
        var actorEsSuperAdmin = EsSuperAdmin(actorRoles);
        var correoSolicitado = string.IsNullOrWhiteSpace(cambios.Correo) ? actual.Correo : cambios.Correo.ToLowerInvariant();

        var correoDuplicado = usuarios.Any(u =>
            u.Id != id &&
            u.Correo.Equals(correoSolicitado, StringComparison.OrdinalIgnoreCase)
        );
        if (correoDuplicado)
        {
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");
        }

        var actualizado = new Usuario
        {
            Id = id,
            Nombre = string.IsNullOrWhiteSpace(cambios.Nombre) ? actual.Nombre : cambios.Nombre,
            Correo = correoSolicitado,
            PasswordHash = string.IsNullOrWhiteSpace(cambios.PasswordHash) || !puedeCambiarPassword
                ? actual.PasswordHash
                : cambios.PasswordHash,
            Estado = actorEsSuperAdmin && !string.IsNullOrWhiteSpace(cambios.Estado) ? cambios.Estado : actual.Estado,
            Roles = actorEsSuperAdmin && cambios.Roles.Count > 0 ? cambios.Roles : actual.Roles
        };

        usuarios[index] = actualizado;
        await _store.WriteAsync(usuarios);
        return actualizado;
    }

    public async Task<Usuario?> ToggleEstadoAsync(int id, string? forzarEstado = null, int? actorId = null, IEnumerable<string>? actorRoles = null)
    {
        var usuarios = await ObtenerTodosAsync();
        var index = usuarios.FindIndex(u => u.Id == id);
        if (index < 0)
        {
            return null;
        }

        if (!EsSuperAdmin(actorRoles))
        {
            throw new InvalidOperationException("Solo un SuperAdmin puede inactivar o activar usuarios.");
        }

        if (actorId.HasValue && actorId.Value == id)
        {
            throw new InvalidOperationException("No puedes cambiar tu propio estado.");
        }

        var actual = usuarios[index];
        var estadoSolicitado = NormalizarEstado(forzarEstado);
        if (string.Equals(estadoSolicitado, EstadoActivo, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Solo se permite inactivar usuarios.");
        }

        if (!EsActivo(actual.Estado))
        {
            return actual;
        }

        var nuevoEstado = EstadoInactivo;

        actual.Estado = nuevoEstado;
        usuarios[index] = actual;
        await _store.WriteAsync(usuarios);
        return actual;
    }

    public async Task<Usuario?> ActualizarPasswordPorCorreoAsync(string correo, string nuevaPassword)
    {
        var usuarios = await ObtenerTodosAsync();
        var index = usuarios.FindIndex(u => u.Correo.Equals(correo.Trim(), StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return null;
        }

        var actual = usuarios[index];
        actual.PasswordHash = nuevaPassword;
        usuarios[index] = actual;
        await _store.WriteAsync(usuarios);
        return actual;
    }

    private static List<Usuario> DefaultUsuarios()
    {
        return
        [
            new Usuario
            {
                Id = 1,
                Nombre = "Admin",
                Correo = "admin@cafeuna.com",
                PasswordHash = "admin123",
                Estado = "activo",
                Roles = ["SuperAdmin"]
            }
        ];
    }

    private static bool EsActivo(string? estado) =>
        string.Equals((estado ?? string.Empty).Trim(), EstadoActivo, StringComparison.OrdinalIgnoreCase);

    private static bool EsSuperAdmin(IEnumerable<string>? roles) =>
        roles?.Any(r => string.Equals((r ?? string.Empty).Trim(), RolSuperAdmin, StringComparison.OrdinalIgnoreCase)) == true;

    private static string? NormalizarEstado(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            return null;
        }

        return string.Equals(estado.Trim(), EstadoInactivo, StringComparison.OrdinalIgnoreCase)
            ? EstadoInactivo
            : EstadoActivo;
    }
}
