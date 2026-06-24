namespace PruebaTecnicaCip.Web.Integrations.Colegiados;

public interface IColegiadosClient
{
    Task<ColegiadoDto?> FindByDniAsync(string dni, CancellationToken cancellationToken = default);
}
