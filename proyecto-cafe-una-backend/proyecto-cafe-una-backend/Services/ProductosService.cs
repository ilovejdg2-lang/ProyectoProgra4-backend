using proyecto_cafe_una_backend.Models;

namespace proyecto_cafe_una_backend.Services;

public class ProductosService
{
    private const decimal IvaRate = 0.13m;
    private readonly JsonFileStore _store = new("productos.json");

    public static int CalcularPrecioConIva(int precioNormal)
    {
        return (int)Math.Round(precioNormal * (1 + IvaRate), MidpointRounding.AwayFromZero);
    }

    public async Task<List<Producto>> ObtenerTodosAsync()
    {
        var productos = await _store.ReadAsync(() => new List<Producto>());
        return productos.Select(NormalizarProducto).ToList();
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        var productos = await ObtenerTodosAsync();
        return productos.FirstOrDefault(p => p.Id == id);
    }

    public async Task<Producto> CrearAsync(Producto nuevoProducto)
    {
        var productos = await ObtenerTodosAsync();
        var nextId = productos.Count == 0 ? 1 : productos.Max(p => p.Id) + 1;

        var productoCompleto = NormalizarProducto(new Producto
        {
            Id = nextId,
            Nombre = nuevoProducto.Nombre,
            Descripcion = nuevoProducto.Descripcion,
            Peso = nuevoProducto.Peso,
            Imagen = nuevoProducto.Imagen,
            PrecioNormal = nuevoProducto.PrecioNormal,
            Stock = nuevoProducto.Stock,
            Estado = nuevoProducto.Estado
        });

        productos.Add(productoCompleto);
        await _store.WriteAsync(productos);
        return productoCompleto;
    }

    public async Task<Producto?> ActualizarAsync(int id, Producto cambios)
    {
        var productos = await ObtenerTodosAsync();
        var index = productos.FindIndex(p => p.Id == id);
        if (index < 0)
        {
            return null;
        }

        var actual = productos[index];
        var actualizado = NormalizarProducto(new Producto
        {
            Id = id,
            Nombre = string.IsNullOrWhiteSpace(cambios.Nombre) ? actual.Nombre : cambios.Nombre,
            Descripcion = cambios.Descripcion ?? actual.Descripcion,
            Peso = cambios.Peso ?? actual.Peso,
            Imagen = cambios.Imagen ?? actual.Imagen,
            PrecioNormal = cambios.PrecioNormal,
            Stock = cambios.Stock,
            Estado = string.IsNullOrWhiteSpace(cambios.Estado) ? actual.Estado : cambios.Estado
        });

        productos[index] = actualizado;
        await _store.WriteAsync(productos);
        return actualizado;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var productos = await ObtenerTodosAsync();
        var filtrados = productos.Where(p => p.Id != id).ToList();
        if (filtrados.Count == productos.Count)
        {
            return false;
        }

        await _store.WriteAsync(filtrados);
        return true;
    }

    public async Task<List<Producto>> AjustarStockAsync(IEnumerable<AjusteStockItem> carritoItems)
    {
        var productos = await ObtenerTodosAsync();
        var cambiosPorId = carritoItems
            .Where(item => item.Id > 0 && item.Units > 0)
            .GroupBy(item => item.Id)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Units));

        var faltantes = new List<string>();
        foreach (var producto in productos)
        {
            if (!cambiosPorId.TryGetValue(producto.Id, out var unidadesCompradas) || unidadesCompradas <= 0)
            {
                continue;
            }

            if (producto.Estado == "Deshabilitado")
            {
                faltantes.Add($"{producto.Nombre} (deshabilitado)");
                continue;
            }

            if (producto.Stock <= 0)
            {
                faltantes.Add($"{producto.Nombre} (sin stock)");
                continue;
            }

            if (producto.Stock < unidadesCompradas)
            {
                faltantes.Add($"{producto.Nombre} ({producto.Stock} disponibles, solicitadas {unidadesCompradas})");
            }
        }

        if (faltantes.Count > 0)
        {
            throw new InvalidOperationException(
                $"No se puede completar la compra. Stock insuficiente para: {string.Join(", ", faltantes)}"
            );
        }

        var actualizados = productos
            .Select(producto =>
            {
                if (!cambiosPorId.TryGetValue(producto.Id, out var unidadesCompradas) || unidadesCompradas <= 0)
                {
                    return producto;
                }

                producto.Stock = Math.Max(producto.Stock - unidadesCompradas, 0);
                return NormalizarProducto(producto);
            })
            .ToList();

        await _store.WriteAsync(actualizados);
        return actualizados;
    }

    private static Producto NormalizarProducto(Producto producto)
    {
        var precioNormal = Math.Max(producto.PrecioNormal, 0);
        var stock = Math.Max(producto.Stock, 0);
        var estado = producto.Estado == "Deshabilitado" ? "Deshabilitado" : "Habilitado";

        return new Producto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            Peso = producto.Peso,
            Imagen = producto.Imagen,
            PrecioNormal = precioNormal,
            PrecioConIva = CalcularPrecioConIva(precioNormal),
            Stock = stock,
            Estado = estado
        };
    }
}
