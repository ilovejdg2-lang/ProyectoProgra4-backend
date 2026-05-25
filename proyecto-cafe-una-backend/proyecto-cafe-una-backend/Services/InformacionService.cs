using System.Text.Json.Nodes;

namespace proyecto_cafe_una_backend.Services;

public class InformacionService
{
    private readonly JsonFileStore _store = new("informacion.json");

    public async Task<JsonObject> ObtenerInformacionAsync()
    {
        return await _store.ReadAsync(DefaultInformacion);
    }

    public async Task<JsonNode?> ObtenerSeccionAsync(string seccion)
    {
        var info = await ObtenerInformacionAsync();
        return info[seccion]?.DeepClone();
    }

    public async Task<JsonObject> ActualizarInformacionAsync(JsonObject nuevaInformacion)
    {
        await _store.WriteAsync(nuevaInformacion);
        return nuevaInformacion;
    }

    public async Task<JsonObject> ActualizarSeccionAsync(string seccion, JsonObject cambios)
    {
        var info = await ObtenerInformacionAsync();
        var current = info[seccion] as JsonObject ?? new JsonObject();

        foreach (var pair in cambios)
        {
            current[pair.Key] = pair.Value?.DeepClone();
        }

        info[seccion] = current;
        await _store.WriteAsync(info);
        return current;
    }

    public async Task<JsonObject> AgregarGaleriaItemAsync(JsonObject item)
    {
        var info = await ObtenerInformacionAsync();
        var gallery = info["gallery"] as JsonArray ?? new JsonArray();

        var nuevo = item.DeepClone() as JsonObject ?? new JsonObject();
        nuevo["id"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        gallery.Add(nuevo);
        info["gallery"] = gallery;
        await _store.WriteAsync(info);
        return nuevo;
    }

    public async Task<JsonObject?> ActualizarGaleriaItemAsync(long id, JsonObject cambios)
    {
        var info = await ObtenerInformacionAsync();
        var gallery = info["gallery"] as JsonArray ?? new JsonArray();

        JsonObject? actualizado = null;
        for (var i = 0; i < gallery.Count; i++)
        {
            if (gallery[i] is not JsonObject item)
            {
                continue;
            }

            var itemId = item["id"]?.GetValue<long>() ?? 0;
            if (itemId != id)
            {
                continue;
            }

            foreach (var pair in cambios)
            {
                item[pair.Key] = pair.Value?.DeepClone();
            }

            actualizado = item;
            gallery[i] = item;
            break;
        }

        if (actualizado is null)
        {
            return null;
        }

        info["gallery"] = gallery;
        await _store.WriteAsync(info);
        return actualizado;
    }

    public async Task<bool> EliminarGaleriaItemAsync(long id)
    {
        var info = await ObtenerInformacionAsync();
        var gallery = info["gallery"] as JsonArray ?? new JsonArray();

        var originalCount = gallery.Count;
        for (var i = gallery.Count - 1; i >= 0; i--)
        {
            if (gallery[i] is not JsonObject item)
            {
                continue;
            }

            var itemId = item["id"]?.GetValue<long>() ?? 0;
            if (itemId == id)
            {
                gallery.RemoveAt(i);
            }
        }

        var removed = gallery.Count != originalCount;
        if (!removed)
        {
            return false;
        }

        info["gallery"] = gallery;
        await _store.WriteAsync(info);
        return true;
    }

    private static JsonObject DefaultInformacion()
    {
        return new JsonObject
        {
            ["hero"] = new JsonObject
            {
                ["title"] = "Cafe UNA",
                ["subtitle"] = "Cafe de especialidad con calidad universitaria.",
                ["buttonText"] = "Conocer mas",
                ["backgroundImage"] = string.Empty
            },
            ["mission"] = new JsonObject
            {
                ["title"] = "Mision",
                ["description"] = "Fomentar el desarrollo sostenible del cafe con impacto social."
            },
            ["vision"] = new JsonObject
            {
                ["title"] = "Vision",
                ["description"] = "Ser referencia en cafe sostenible y colaboracion comunitaria."
            },
            ["gallery"] = new JsonArray()
        };
    }
}
