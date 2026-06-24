using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using PruebaTecnicaCip.Web.Data;
using PruebaTecnicaCip.Web.Domain;

namespace PruebaTecnicaCip.Tests;

public class ApplicationDbContextTests
{
    [Fact]
    public void ModelDefinesSeedEvent()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var designTimeModel = context.GetService<IDesignTimeModel>().Model;
        var seedEvent = designTimeModel.FindEntityType(typeof(InstitutionalEvent))!
            .GetSeedData()
            .Single();

        Assert.Equal(1, seedEvent[nameof(InstitutionalEvent.Id)]);
        Assert.Equal("Dia del Padre", seedEvent[nameof(InstitutionalEvent.Name)]);
        Assert.Equal("Lima", seedEvent[nameof(InstitutionalEvent.Council)]);
        Assert.Equal(2, seedEvent[nameof(InstitutionalEvent.Capacity)]);
    }

    [Fact]
    public void RegistrationRequestStatusIsStoredAsString()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var statusProperty = context.Model.FindEntityType(typeof(RegistrationRequest))!
            .FindProperty(nameof(RegistrationRequest.Status))!;

        Assert.Equal(typeof(string), statusProperty.GetProviderClrType());
    }
}
