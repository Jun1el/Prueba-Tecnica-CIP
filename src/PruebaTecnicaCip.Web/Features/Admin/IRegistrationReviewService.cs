namespace PruebaTecnicaCip.Web.Features.Admin;

public interface IRegistrationReviewService
{
    Task<RegistrationReviewResult> RejectAsync(int requestId, string observation, CancellationToken cancellationToken = default);
}
