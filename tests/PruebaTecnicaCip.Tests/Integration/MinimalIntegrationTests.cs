using System.Net;

namespace PruebaTecnicaCip.Tests.Integration;

public class MinimalIntegrationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task HomePageRendersRegistrationForm()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Inscripcion Dia del Padre", content);
        Assert.Contains("Enviar solicitud", content);
    }

    [Fact]
    public async Task AdminDashboardRendersPendingRequests()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Admin");
        var content = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, content);
        Assert.Contains("Dashboard administrador", content);
        Assert.Contains("Juan Perez", content);
        Assert.Contains("Aprobar", content);
    }
}
