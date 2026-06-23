namespace PruebaTecnicaCip.Web.Domain;

public class RegistrationRequest
{
    public int Id { get; set; }

    public int InstitutionalEventId { get; set; }

    public InstitutionalEvent? InstitutionalEvent { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Council { get; set; } = string.Empty;

    public string ChildDniImagePath { get; set; } = string.Empty;

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
