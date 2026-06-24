namespace PruebaTecnicaCip.Web.Features.Admin;

public interface IRegistrationReviewService
{
    Task<RegistrationReviewResult> ApproveAsync(int requestId, CancellationToken cancellationToken = default);

    Task<RegistrationReviewResult> RejectAsync(int requestId, string observation, CancellationToken cancellationToken = default);
}
