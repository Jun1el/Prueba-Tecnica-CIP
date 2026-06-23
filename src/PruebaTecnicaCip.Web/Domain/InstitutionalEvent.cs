namespace PruebaTecnicaCip.Web.Domain;

public class InstitutionalEvent
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Council { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public int ApprovedCount { get; set; }

    public DateTime StartsAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public ICollection<RegistrationRequest> RegistrationRequests { get; set; } = [];
}
