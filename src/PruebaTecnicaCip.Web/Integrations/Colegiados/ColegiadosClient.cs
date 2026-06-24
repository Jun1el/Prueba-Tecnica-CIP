using System.Net.Http.Json;

namespace PruebaTecnicaCip.Web.Integrations.Colegiados;

public sealed class ColegiadosClient(HttpClient httpClient) : IColegiadosClient
{
    public async Task<ColegiadoDto?> FindByDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dni);

        var endpoint = $"colegiados?dni={Uri.EscapeDataString(dni.Trim())}";
        var response = await httpClient.GetFromJsonAsync<List<ColegiadoDto>>(endpoint, cancellationToken);

        return response?.FirstOrDefault();
    }
}
