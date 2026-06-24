using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PruebaTecnicaCip.Web.Data;
using PruebaTecnicaCip.Web.Domain;
using PruebaTecnicaCip.Web.Features.Admin;

namespace PruebaTecnicaCip.Tests;

public class RegistrationReviewServiceTests
{
    [Fact]
    public async Task RejectAsyncRequiresObservation()
    {
        using var dbContext = CreateDbContext();
        var service = new RegistrationReviewService(dbContext, NullLogger<RegistrationReviewService>.Instance);

        var result = await service.RejectAsync(1, " ");

        Assert.False(result.Succeeded);
        Assert.Equal("La observacion es obligatoria para rechazar una solicitud.", result.Message);
    }

    [Fact]
    public async Task RejectAsyncUpdatesPendingRequest()
    {
        using var dbContext = CreateDbContext();
        dbContext.RegistrationRequests.Add(CreateRequest());
        await dbContext.SaveChangesAsync();
        var service = new RegistrationReviewService(dbContext, NullLogger<RegistrationReviewService>.Instance);

        var result = await service.RejectAsync(1, "No cumple documentacion");

        var request = await dbContext.RegistrationRequests.SingleAsync();
        Assert.True(result.Succeeded);
        Assert.Equal(RegistrationStatus.Rejected, request.Status);
        Assert.Equal("No cumple documentacion", request.RejectionReason);
        Assert.NotNull(request.ReviewedAt);
    }

    [Fact]
    public async Task RejectAsyncOnlyAllowsPendingRequests()
    {
        using var dbContext = CreateDbContext();
        dbContext.RegistrationRequests.Add(CreateRequest(status: RegistrationStatus.Approved));
        await dbContext.SaveChangesAsync();
        var service = new RegistrationReviewService(dbContext, NullLogger<RegistrationReviewService>.Instance);

        var result = await service.RejectAsync(1, "Observacion");

        Assert.False(result.Succeeded);
        Assert.Equal("Solo se pueden rechazar solicitudes pendientes.", result.Message);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static RegistrationRequest CreateRequest(RegistrationStatus status = RegistrationStatus.Pending)
    {
        return new RegistrationRequest
        {
            Id = 1,
            InstitutionalEventId = 1,
            Dni = "12345678",
            FullName = "Juan Perez",
            Council = "Lima",
            ChildDniImagePath = "/uploads/child-dni/test.png",
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }
}
