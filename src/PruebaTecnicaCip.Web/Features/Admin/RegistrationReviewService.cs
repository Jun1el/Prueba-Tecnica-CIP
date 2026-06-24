using Microsoft.EntityFrameworkCore;
using PruebaTecnicaCip.Web.Data;
using PruebaTecnicaCip.Web.Domain;

namespace PruebaTecnicaCip.Web.Features.Admin;

public sealed class RegistrationReviewService(
    ApplicationDbContext dbContext,
    ILogger<RegistrationReviewService> logger) : IRegistrationReviewService
{
    public async Task<RegistrationReviewResult> RejectAsync(
        int requestId,
        string observation,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(observation))
        {
            return RegistrationReviewResult.Failure("La observacion es obligatoria para rechazar una solicitud.");
        }

        var request = await dbContext.RegistrationRequests
            .SingleOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (request is null)
        {
            return RegistrationReviewResult.Failure("La solicitud no existe.");
        }

        if (request.Status != RegistrationStatus.Pending)
        {
            return RegistrationReviewResult.Failure("Solo se pueden rechazar solicitudes pendientes.");
        }

        request.Status = RegistrationStatus.Rejected;
        request.RejectionReason = observation.Trim();
        request.ReviewedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Solicitud {RequestId} rechazada administrativamente. DNI: {Dni}. Observacion: {Observation}",
            request.Id,
            request.Dni,
            request.RejectionReason);

        return RegistrationReviewResult.Success("Solicitud rechazada correctamente.");
    }
}
