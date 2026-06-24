using PruebaTecnicaCip.Web.Integrations.Colegiados;

namespace PruebaTecnicaCip.Web.Features.Eligibility;

public interface IColegiadoEligibilityService
{
    ColegiadoEligibilityResult Evaluate(ColegiadoDto? colegiado);
}
