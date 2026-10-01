namespace E2E.Tests;

public class CongeAppHealthCheckTests
{
    private const string AppUrl = "https://aouzgaga.github.io/formation-gh-api/";

    [Fact]
    public async Task L_application_de_saisie_de_conges_repond_correctement()
    {
        using var client = new HttpClient();

        using var response = await client.GetAsync(AppUrl);

        Assert.True(
            response.IsSuccessStatusCode,
            $"Expected a successful response from {AppUrl} but got {(int)response.StatusCode} {response.StatusCode}.");
    }
}
