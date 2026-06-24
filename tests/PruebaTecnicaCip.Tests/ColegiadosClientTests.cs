using System.Net;
using PruebaTecnicaCip.Web.Integrations.Colegiados;

namespace PruebaTecnicaCip.Tests;

public class ColegiadosClientTests
{
    [Fact]
    public async Task FindByDniAsyncReturnsMatchingColegiado()
    {
        var handler = new StubHttpMessageHandler("""[{"dni":"12345678","nombre":"Juan Perez","habilitado":true,"es_administrativo":false,"consejo_departamental":"Lima"}]""");
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://colegiados-api:3000")
        };
        var client = new ColegiadosClient(httpClient);

        var colegiado = await client.FindByDniAsync("12345678");

        Assert.NotNull(colegiado);
        Assert.Equal("12345678", colegiado.Dni);
        Assert.Equal("Juan Perez", colegiado.Nombre);
        Assert.True(colegiado.Habilitado);
        Assert.False(colegiado.EsAdministrativo);
        Assert.Equal("Lima", colegiado.ConsejoDepartamental);
        Assert.Equal("http://colegiados-api:3000/colegiados?dni=12345678", handler.RequestUri!.ToString());
    }

    [Fact]
    public async Task FindByDniAsyncReturnsNullWhenApiDoesNotFindColegiado()
    {
        var handler = new StubHttpMessageHandler("[]");
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://colegiados-api:3000")
        };
        var client = new ColegiadosClient(httpClient);

        var colegiado = await client.FindByDniAsync("00000000");

        Assert.Null(colegiado);
    }

    [Fact]
    public async Task FindByDniAsyncRejectsEmptyDni()
    {
        var handler = new StubHttpMessageHandler("[]");
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://colegiados-api:3000")
        };
        var client = new ColegiadosClient(httpClient);

        await Assert.ThrowsAsync<ArgumentException>(() => client.FindByDniAsync(" "));
    }

    private sealed class StubHttpMessageHandler(string responseContent) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            });
        }
    }
}
