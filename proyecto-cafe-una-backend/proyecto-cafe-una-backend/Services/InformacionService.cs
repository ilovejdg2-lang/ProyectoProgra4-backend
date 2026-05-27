using System.Text.Json.Nodes;

namespace proyecto_cafe_una_backend.Services;

public class InformacionService
{
    private readonly JsonFileStore _store = new("informacion.json");

    public async Task<JsonObject> ObtenerInformacionAsync()
    {
        var info = await _store.ReadAsync(DefaultInformacion);
        if (CompletarEstructura(info))
        {
            await _store.WriteAsync(info);
        }

        return info;
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
            ["historia"] = new JsonObject
            {
                ["title"] = "Nuestra historia",
                ["description"] = "Nacimos para impulsar la cultura cafetalera con identidad universitaria, trabajo colaborativo y desarrollo sostenible."
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
            ["gallery"] = DefaultGallery()
        };
    }

    private static JsonArray DefaultGallery()
    {
        return new JsonArray
        {
            new JsonObject
            {
                ["id"] = 1,
                ["title"] = "Latte art",
                ["image"] = "https://images.unsplash.com/photo-1498804103079-a6351b050096?auto=format&fit=crop&w=800&q=80"
            },
            new JsonObject
            {
                ["id"] = 2,
                ["title"] = "Interior de cafetería",
                ["image"] = "https://images.unsplash.com/photo-1521017432531-fbd92d768814?auto=format&fit=crop&w=800&q=80"
            },
            new JsonObject
            {
                ["id"] = 3,
                ["title"] = "Granos de café",
                ["image"] = "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=80"
            }
        };
    }

    private static bool CompletarEstructura(JsonObject info)
    {
        var defaults = DefaultInformacion();
        var updated = false;

        foreach (var entry in defaults)
        {
            var key = entry.Key;
            var defaultValue = entry.Value?.DeepClone();
            var currentValue = info[key];

            if (defaultValue is JsonArray)
            {
                if (currentValue is not JsonArray currentArray)
                {
                    info[key] = defaultValue;
                    updated = true;
                }
                else if (currentArray.Count == 0 && key == "gallery")
                {
                    info[key] = defaultValue;
                    updated = true;
                }

                continue;
            }

            if (defaultValue is JsonObject defaultObject)
            {
                if (currentValue is JsonObject currentObject)
                {
                    foreach (var prop in defaultObject)
                    {
                        if (currentObject[prop.Key] is null)
                        {
                            currentObject[prop.Key] = prop.Value?.DeepClone();
                            updated = true;
                        }
                    }

                    continue;
                }

                info[key] = defaultObject.DeepClone();
                updated = true;
            }
        }

        return updated;
    }
}
