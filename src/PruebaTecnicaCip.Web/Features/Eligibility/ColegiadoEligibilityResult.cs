namespace PruebaTecnicaCip.Web.Features.Eligibility;

public sealed record ColegiadoEligibilityResult(bool IsEligible, string? Reason)
{
    public static ColegiadoEligibilityResult Eligible() => new(true, null);

    public static ColegiadoEligibilityResult Ineligible(string reason) => new(false, reason);
}
