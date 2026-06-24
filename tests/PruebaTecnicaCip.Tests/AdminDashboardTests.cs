using Microsoft.EntityFrameworkCore;
using PruebaTecnicaCip.Web.Data;
using PruebaTecnicaCip.Web.Domain;
using PruebaTecnicaCip.Web.Features.Admin;
using PruebaTecnicaCip.Web.Pages.Admin;

namespace PruebaTecnicaCip.Tests;

public class AdminDashboardTests
{
    [Fact]
    public async Task OnGetAsyncBuildsMetricsAndPendingList()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new ApplicationDbContext(options);
        dbContext.InstitutionalEvents.Add(new InstitutionalEvent
        {
            Id = 1,
            Name = "Dia del Padre",
            Council = "Lima",
            Capacity = 2,
            ApprovedCount = 1,
            StartsAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        dbContext.RegistrationRequests.AddRange(
            CreateRequest(1, "12345678", RegistrationStatus.Pending, DateTime.UtcNow.AddMinutes(-2)),
            CreateRequest(2, "99887766", RegistrationStatus.Approved, DateTime.UtcNow.AddMinutes(-1)),
            CreateRequest(3, "11223344", RegistrationStatus.Rejected, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        var page = new IndexModel(dbContext, new NoOpRegistrationReviewService());

        await page.OnGetAsync(CancellationToken.None);

        Assert.Equal("Dia del Padre", page.Metrics.EventName);
        Assert.Equal(2, page.Metrics.Capacity);
        Assert.Equal(1, page.Metrics.ApprovedCount);
        Assert.Equal(1, page.Metrics.RemainingCapacity);
        Assert.Equal(1, page.Metrics.PendingCount);
        Assert.Equal(1, page.Metrics.RejectedCount);
        Assert.Single(page.PendingRegistrations);
        Assert.Equal("12345678", page.PendingRegistrations[0].Dni);
    }

    private static RegistrationRequest CreateRequest(int id, string dni, RegistrationStatus status, DateTime createdAt)
    {
        return new RegistrationRequest
        {
            Id = id,
            InstitutionalEventId = 1,
            Dni = dni,
            FullName = $"Nombre {dni}",
            Council = "Lima",
            ChildDniImagePath = "/uploads/child-dni/test.png",
            Status = status,
            CreatedAt = createdAt
        };
    }

    private sealed class NoOpRegistrationReviewService : IRegistrationReviewService
    {
        public Task<RegistrationReviewResult> RejectAsync(int requestId, string observation, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RegistrationReviewResult.Success("OK"));
        }
    }
}
