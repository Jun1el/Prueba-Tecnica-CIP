using PruebaTecnicaCip.Web.Integrations.Colegiados;

namespace PruebaTecnicaCip.Web.Features.Eligibility;

public sealed class ColegiadoEligibilityService : IColegiadoEligibilityService
{
    private const string RequiredCouncil = "Lima";

    public ColegiadoEligibilityResult Evaluate(ColegiadoDto? colegiado)
    {
        if (colegiado is null)
        {
            return ColegiadoEligibilityResult.Ineligible("No se encontro un colegiado con el DNI ingresado.");
        }

        if (!colegiado.Habilitado)
        {
            return ColegiadoEligibilityResult.Ineligible("El colegiado no se encuentra habilitado.");
        }

        if (!string.Equals(colegiado.ConsejoDepartamental, RequiredCouncil, StringComparison.OrdinalIgnoreCase))
        {
            return ColegiadoEligibilityResult.Ineligible("El colegiado no pertenece al Consejo Departamental de Lima.");
        }

        if (colegiado.EsAdministrativo)
        {
            return ColegiadoEligibilityResult.Ineligible("El personal administrativo no puede inscribirse.");
        }

        return ColegiadoEligibilityResult.Eligible();
    }
}
