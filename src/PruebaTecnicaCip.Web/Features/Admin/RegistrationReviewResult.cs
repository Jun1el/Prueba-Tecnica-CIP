namespace PruebaTecnicaCip.Web.Features.Admin;

public sealed record RegistrationReviewResult(bool Succeeded, string Message)
{
    public static RegistrationReviewResult Success(string message) => new(true, message);

    public static RegistrationReviewResult Failure(string message) => new(false, message);
}
