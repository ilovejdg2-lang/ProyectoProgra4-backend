using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class UsuariosService
{
    private readonly JsonFileStore _store = new("usuarios.json");

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        return await _store.ReadAsync(DefaultUsuarios);
    }

    public async Task<List<Usuario>> ObtenerActivosAsync()
    {
        var usuarios = await ObtenerTodosAsync();
        return usuarios.Where(u => u.Estado == "activo").ToList();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        var usuarios = await ObtenerTodosAsync();
        return usuarios.FirstOrDefault(u => u.Id == id);
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

    public async Task<Usuario?> ToggleEstadoAsync(int id, string? forzarEstado = null)
    {
        var usuarios = await ObtenerTodosAsync();
        var index = usuarios.FindIndex(u => u.Id == id);
        if (index < 0)
        {
            return null;
        }

        var actual = usuarios[index];
        var nuevoEstado = string.IsNullOrWhiteSpace(forzarEstado)
            ? (actual.Estado == "activo" ? "inactivo" : "activo")
            : forzarEstado;

        actual.Estado = nuevoEstado;
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
}
