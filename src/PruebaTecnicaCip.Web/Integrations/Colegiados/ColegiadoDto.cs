using System.Text.Json.Serialization;

namespace PruebaTecnicaCip.Web.Integrations.Colegiados;

public sealed record ColegiadoDto
{
    [JsonPropertyName("dni")]
    public string Dni { get; init; } = string.Empty;

    [JsonPropertyName("nombre")]
    public string Nombre { get; init; } = string.Empty;

    [JsonPropertyName("habilitado")]
    public bool Habilitado { get; init; }

    [JsonPropertyName("es_administrativo")]
    public bool EsAdministrativo { get; init; }

    [JsonPropertyName("consejo_departamental")]
    public string ConsejoDepartamental { get; init; } = string.Empty;
}
