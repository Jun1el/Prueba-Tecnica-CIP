using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PruebaTecnicaCip.Web.Data;
using PruebaTecnicaCip.Web.Domain;
using PruebaTecnicaCip.Web.Integrations.Colegiados;

namespace PruebaTecnicaCip.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryDatabaseRoot _databaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IColegiadosClient>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("cip-integration", _databaseRoot));
            services.AddScoped<IColegiadosClient, StubColegiadosClient>();

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.InstitutionalEvents.Add(new InstitutionalEvent
            {
                Id = 1,
                Name = "Dia del Padre",
                Council = "Lima",
                Capacity = 2,
                ApprovedCount = 0,
                StartsAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
            dbContext.RegistrationRequests.Add(new RegistrationRequest
            {
                InstitutionalEventId = 1,
                Dni = "12345678",
                FullName = "Juan Perez",
                Council = "Lima",
                ChildDniImagePath = "/uploads/child-dni/test.png",
                Status = RegistrationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
            dbContext.SaveChanges();
        });
    }

    private sealed class StubColegiadosClient : IColegiadosClient
    {
        public Task<ColegiadoDto?> FindByDniAsync(string dni, CancellationToken cancellationToken = default)
        {
            var colegiado = new ColegiadoDto
            {
                Dni = dni,
                Nombre = "Colegiado Integracion",
                Habilitado = true,
                EsAdministrativo = false,
                ConsejoDepartamental = "Lima"
            };

            return Task.FromResult<ColegiadoDto?>(colegiado);
        }
    }
}
