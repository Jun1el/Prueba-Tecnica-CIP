using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaCip.Web.Data;
using PruebaTecnicaCip.Web.Domain;

namespace PruebaTecnicaCip.Web.Pages.Admin;

public class IndexModel(ApplicationDbContext dbContext) : PageModel
{
    private const int EventId = 1;

    public DashboardMetrics Metrics { get; private set; } = new();

    public IReadOnlyList<PendingRegistrationItem> PendingRegistrations { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var institutionalEvent = await dbContext.InstitutionalEvents
            .AsNoTracking()
            .SingleAsync(e => e.Id == EventId, cancellationToken);

        var statusCounts = await dbContext.RegistrationRequests
            .AsNoTracking()
            .Where(r => r.InstitutionalEventId == EventId)
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var pending = statusCounts.FirstOrDefault(x => x.Status == RegistrationStatus.Pending)?.Count ?? 0;
        var approved = statusCounts.FirstOrDefault(x => x.Status == RegistrationStatus.Approved)?.Count ?? 0;
        var rejected = statusCounts.FirstOrDefault(x => x.Status == RegistrationStatus.Rejected)?.Count ?? 0;

        Metrics = new DashboardMetrics
        {
            EventName = institutionalEvent.Name,
            Capacity = institutionalEvent.Capacity,
            ApprovedCount = institutionalEvent.ApprovedCount,
            RemainingCapacity = Math.Max(0, institutionalEvent.Capacity - institutionalEvent.ApprovedCount),
            PendingCount = pending,
            RejectedCount = rejected,
            ApprovedRequestsCount = approved
        };

        PendingRegistrations = await dbContext.RegistrationRequests
            .AsNoTracking()
            .Where(r => r.InstitutionalEventId == EventId && r.Status == RegistrationStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new PendingRegistrationItem
            {
                Id = r.Id,
                Dni = r.Dni,
                FullName = r.FullName,
                Council = r.Council,
                CreatedAt = r.CreatedAt,
                ChildDniImagePath = r.ChildDniImagePath
            })
            .ToListAsync(cancellationToken);
    }

    public sealed record DashboardMetrics
    {
        public string EventName { get; init; } = string.Empty;

        public int Capacity { get; init; }

        public int ApprovedCount { get; init; }

        public int RemainingCapacity { get; init; }

        public int PendingCount { get; init; }

        public int RejectedCount { get; init; }

        public int ApprovedRequestsCount { get; init; }
    }

    public sealed record PendingRegistrationItem
    {
        public int Id { get; init; }

        public string Dni { get; init; } = string.Empty;

        public string FullName { get; init; } = string.Empty;

        public string Council { get; init; } = string.Empty;

        public DateTime CreatedAt { get; init; }

        public string ChildDniImagePath { get; init; } = string.Empty;
    }
}
