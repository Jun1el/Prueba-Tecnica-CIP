using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PruebaTecnicaCip.Web.Data;
using PruebaTecnicaCip.Web.Domain;
using PruebaTecnicaCip.Web.Features.Eligibility;
using PruebaTecnicaCip.Web.Integrations.Colegiados;
using PruebaTecnicaCip.Web.Pages;

namespace PruebaTecnicaCip.Tests;

public class RegistrationPortalTests
{
    [Fact]
    public async Task OnPostAsyncStoresPendingRequestWhenColegiadoIsEligible()
    {
        using var fixture = RegistrationPortalFixture.CreateEligible();
        var page = fixture.CreatePage();
        page.Input = CreateInput("12345678", "Juan Perez");

        var result = await page.OnPostAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(result);

        var request = await fixture.DbContext.RegistrationRequests.SingleAsync();
        Assert.Equal("12345678", request.Dni);
        Assert.Equal("Juan Perez", request.FullName);
        Assert.Equal("Lima", request.Council);
        Assert.Equal(RegistrationStatus.Pending, request.Status);
        Assert.Null(request.RejectionReason);
        Assert.StartsWith("/uploads/child-dni/12345678-", request.ChildDniImagePath);
        Assert.True(File.Exists(Path.Combine(fixture.WebRootPath, request.ChildDniImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task OnPostAsyncStoresRejectedRequestWhenColegiadoIsNotEligible()
    {
        using var fixture = RegistrationPortalFixture.CreateDisabled();
        var page = fixture.CreatePage();
        page.Input = CreateInput("11223344", "Carlos Ruiz");

        var result = await page.OnPostAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(result);

        var request = await fixture.DbContext.RegistrationRequests.SingleAsync();
        Assert.Equal(RegistrationStatus.Rejected, request.Status);
        Assert.Equal("El colegiado no se encuentra habilitado.", request.RejectionReason);
        Assert.NotNull(request.ReviewedAt);
    }

    [Fact]
    public async Task OnPostAsyncBlocksDuplicateDniForSameEvent()
    {
        using var fixture = RegistrationPortalFixture.CreateEligible();
        fixture.DbContext.RegistrationRequests.Add(new RegistrationRequest
        {
            InstitutionalEventId = 1,
            Dni = "12345678",
            FullName = "Juan Perez",
            Council = "Lima",
            ChildDniImagePath = "/uploads/child-dni/existing.png",
            Status = RegistrationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await fixture.DbContext.SaveChangesAsync();

        var page = fixture.CreatePage();
        page.Input = CreateInput("12345678", "Juan Perez");

        var result = await page.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal(1, await fixture.DbContext.RegistrationRequests.CountAsync());
        Assert.False(page.ModelState.IsValid);
    }

    [Fact]
    public async Task OnPostAsyncBlocksRegistrationWhenEventIsFull()
    {
        using var fixture = RegistrationPortalFixture.CreateEligible(capacity: 2, approvedCount: 2);
        var page = fixture.CreatePage();
        page.Input = CreateInput("12345678", "Juan Perez");

        var result = await page.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Empty(fixture.DbContext.RegistrationRequests);
        Assert.True(page.IsFull);
        Assert.False(page.ModelState.IsValid);
    }

    [Fact]
    public async Task OnGetAsyncMarksPageAsFullWhenCapacityIsConsumed()
    {
        using var fixture = RegistrationPortalFixture.CreateEligible(capacity: 2, approvedCount: 2);
        var page = fixture.CreatePage();

        await page.OnGetAsync(CancellationToken.None);

        Assert.True(page.IsFull);
    }

    private static IndexModel.RegistrationInput CreateInput(string dni, string fullName)
    {
        return new IndexModel.RegistrationInput
        {
            Dni = dni,
            FullName = fullName,
            ChildDniImage = CreateFormFile()
        };
    }

    private static IFormFile CreateFormFile()
    {
        var content = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, content.Length, "childDniImage", "dni-menor.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
    }

    private sealed class RegistrationPortalFixture : IDisposable
    {
        private RegistrationPortalFixture(ColegiadoDto colegiado, int capacity = 2, int approvedCount = 0)
        {
            WebRootPath = Path.Combine(Path.GetTempPath(), $"cip-webroot-{Guid.NewGuid():N}");
            Directory.CreateDirectory(WebRootPath);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            DbContext = new ApplicationDbContext(options);
            DbContext.InstitutionalEvents.Add(new InstitutionalEvent
            {
                Id = 1,
                Name = "Dia del Padre",
                Council = "Lima",
                Capacity = capacity,
                ApprovedCount = approvedCount,
                StartsAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
            DbContext.SaveChanges();

            Colegiado = colegiado;
        }

        public ApplicationDbContext DbContext { get; }

        public string WebRootPath { get; }

        private ColegiadoDto Colegiado { get; }

        public static RegistrationPortalFixture CreateEligible(int capacity = 2, int approvedCount = 0)
        {
            return new RegistrationPortalFixture(new ColegiadoDto
            {
                Dni = "12345678",
                Nombre = "Juan Perez",
                Habilitado = true,
                EsAdministrativo = false,
                ConsejoDepartamental = "Lima"
            }, capacity, approvedCount);
        }

        public static RegistrationPortalFixture CreateDisabled()
        {
            return new RegistrationPortalFixture(new ColegiadoDto
            {
                Dni = "11223344",
                Nombre = "Carlos Ruiz",
                Habilitado = false,
                EsAdministrativo = false,
                ConsejoDepartamental = "Lima"
            });
        }

        public IndexModel CreatePage()
        {
            return new IndexModel(
                DbContext,
                new StubColegiadosClient(Colegiado),
                new ColegiadoEligibilityService(),
                new StubWebHostEnvironment(WebRootPath));
        }

        public void Dispose()
        {
            DbContext.Dispose();

            if (Directory.Exists(WebRootPath))
            {
                Directory.Delete(WebRootPath, recursive: true);
            }
        }
    }

    private sealed class StubColegiadosClient(ColegiadoDto colegiado) : IColegiadosClient
    {
        public Task<ColegiadoDto?> FindByDniAsync(string dni, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ColegiadoDto?>(colegiado.Dni == dni ? colegiado : null);
        }
    }

    private sealed class StubWebHostEnvironment(string webRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "PruebaTecnicaCip.Tests";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public string EnvironmentName { get; set; } = "Development";

        public string WebRootPath { get; set; } = webRootPath;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
