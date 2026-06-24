using PruebaTecnicaCip.Web.Features.Eligibility;
using PruebaTecnicaCip.Web.Integrations.Colegiados;

namespace PruebaTecnicaCip.Tests;

public class ColegiadoEligibilityServiceTests
{
    private readonly ColegiadoEligibilityService _service = new();

    [Fact]
    public void EvaluateReturnsEligibleWhenColegiadoMeetsAllRules()
    {
        var colegiado = CreateColegiado();

        var result = _service.Evaluate(colegiado);

        Assert.True(result.IsEligible);
        Assert.Null(result.Reason);
    }

    [Fact]
    public void EvaluateRejectsUnknownColegiado()
    {
        var result = _service.Evaluate(null);

        Assert.False(result.IsEligible);
        Assert.Equal("No se encontro un colegiado con el DNI ingresado.", result.Reason);
    }

    [Fact]
    public void EvaluateRejectsDisabledColegiado()
    {
        var colegiado = CreateColegiado(habilitado: false);

        var result = _service.Evaluate(colegiado);

        Assert.False(result.IsEligible);
        Assert.Equal("El colegiado no se encuentra habilitado.", result.Reason);
    }

    [Fact]
    public void EvaluateRejectsColegiadoOutsideLimaCouncil()
    {
        var colegiado = CreateColegiado(consejoDepartamental: "Callao");

        var result = _service.Evaluate(colegiado);

        Assert.False(result.IsEligible);
        Assert.Equal("El colegiado no pertenece al Consejo Departamental de Lima.", result.Reason);
    }

    [Fact]
    public void EvaluateRejectsAdministrativeStaff()
    {
        var colegiado = CreateColegiado(esAdministrativo: true);

        var result = _service.Evaluate(colegiado);

        Assert.False(result.IsEligible);
        Assert.Equal("El personal administrativo no puede inscribirse.", result.Reason);
    }

    private static ColegiadoDto CreateColegiado(
        bool habilitado = true,
        bool esAdministrativo = false,
        string consejoDepartamental = "Lima")
    {
        return new ColegiadoDto
        {
            Dni = "12345678",
            Nombre = "Juan Perez",
            Habilitado = habilitado,
            EsAdministrativo = esAdministrativo,
            ConsejoDepartamental = consejoDepartamental
        };
    }
}
