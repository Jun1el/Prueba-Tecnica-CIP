using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using PruebaTecnicaCip.Web.Data;
using PruebaTecnicaCip.Web.Domain;
using PruebaTecnicaCip.Web.Features.Admin;

namespace PruebaTecnicaCip.Tests;

public class RegistrationReviewServiceTests
{
    [Fact]
    public async Task ApproveAsyncConsumesCapacityAndApprovesRequest()
    {
        using var dbContext = CreateDbContext();
        dbContext.InstitutionalEvents.Add(CreateEvent(capacity: 2, approvedCount: 1));
        dbContext.RegistrationRequests.Add(CreateRequest());
        await dbContext.SaveChangesAsync();
        var service = new RegistrationReviewService(dbContext, NullLogger<RegistrationReviewService>.Instance);

        var result = await service.ApproveAsync(1);

        var request = await dbContext.RegistrationRequests.SingleAsync();
        var institutionalEvent = await dbContext.InstitutionalEvents.SingleAsync();
        Assert.True(result.Succeeded);
        Assert.Equal(RegistrationStatus.Approved, request.Status);
        Assert.NotNull(request.ReviewedAt);
        Assert.Equal(2, institutionalEvent.ApprovedCount);
    }

    [Fact]
    public async Task ApproveAsyncRejectsWhenCapacityIsFull()
    {
        using var dbContext = CreateDbContext();
        dbContext.InstitutionalEvents.Add(CreateEvent(capacity: 2, approvedCount: 2));
        dbContext.RegistrationRequests.Add(CreateRequest());
        await dbContext.SaveChangesAsync();
        var service = new RegistrationReviewService(dbContext, NullLogger<RegistrationReviewService>.Instance);

        var result = await service.ApproveAsync(1);

        var request = await dbContext.RegistrationRequests.SingleAsync();
        Assert.False(result.Succeeded);
        Assert.Equal("No hay cupos disponibles para aprobar la solicitud.", result.Message);
        Assert.Equal(RegistrationStatus.Pending, request.Status);
    }

    [Fact]
    public async Task ApproveAsyncOnlyAllowsPendingRequests()
    {
        using var dbContext = CreateDbContext();
        dbContext.InstitutionalEvents.Add(CreateEvent());
        dbContext.RegistrationRequests.Add(CreateRequest(status: RegistrationStatus.Rejected));
        await dbContext.SaveChangesAsync();
        var service = new RegistrationReviewService(dbContext, NullLogger<RegistrationReviewService>.Instance);

        var result = await service.ApproveAsync(1);

        Assert.False(result.Succeeded);
        Assert.Equal("Solo se pueden aprobar solicitudes pendientes.", result.Message);
    }

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
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static InstitutionalEvent CreateEvent(int capacity = 2, int approvedCount = 0)
    {
        return new InstitutionalEvent
        {
            Id = 1,
            Name = "Dia del Padre",
            Council = "Lima",
            Capacity = capacity,
            ApprovedCount = approvedCount,
            StartsAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static RegistrationRequest CreateRequest(RegistrationStatus status = RegistrationStatus.Pending)
    {
        return new RegistrationRequest
        {
            Id = 1,
            InstitutionalEventId = 1,
            InstitutionalEvent = null,
            Dni = "12345678",
            FullName = "Juan Perez",
            Council = "Lima",
            ChildDniImagePath = "/uploads/child-dni/test.png",
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }
}
