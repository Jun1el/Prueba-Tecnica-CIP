using Microsoft.EntityFrameworkCore;
using PruebaTecnicaCip.Web.Data;
using PruebaTecnicaCip.Web.Domain;

namespace PruebaTecnicaCip.Web.Features.Admin;

public sealed class RegistrationReviewService(
    ApplicationDbContext dbContext,
    ILogger<RegistrationReviewService> logger) : IRegistrationReviewService
{
    public async Task<RegistrationReviewResult> ApproveAsync(
        int requestId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var request = await dbContext.RegistrationRequests
                .Include(r => r.InstitutionalEvent)
                .SingleOrDefaultAsync(r => r.Id == requestId, cancellationToken);

            if (request is null)
            {
                return RegistrationReviewResult.Failure("La solicitud no existe.");
            }

            if (request.Status != RegistrationStatus.Pending)
            {
                return RegistrationReviewResult.Failure("Solo se pueden aprobar solicitudes pendientes.");
            }

            if (request.InstitutionalEvent is null)
            {
                return RegistrationReviewResult.Failure("El evento asociado no existe.");
            }

            if (request.InstitutionalEvent.ApprovedCount >= request.InstitutionalEvent.Capacity)
            {
                return RegistrationReviewResult.Failure("No hay cupos disponibles para aprobar la solicitud.");
            }

            request.Status = RegistrationStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;
            request.InstitutionalEvent.ApprovedCount++;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation(
                "Solicitud {RequestId} aprobada. DNI: {Dni}. Cupos aprobados: {ApprovedCount}/{Capacity}",
                request.Id,
                request.Dni,
                request.InstitutionalEvent.ApprovedCount,
                request.InstitutionalEvent.Capacity);

            return RegistrationReviewResult.Success("Solicitud aprobada correctamente.");
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning("Conflicto de concurrencia al aprobar la solicitud {RequestId}.", requestId);

            return RegistrationReviewResult.Failure("La solicitud no pudo aprobarse por un conflicto de concurrencia. Intenta nuevamente.");
        }
    }

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
