using Microsoft.Playwright;

namespace E2E.Tests;

public sealed class CongeAppE2ETests(CongeAppBrowserFixture fixture)
    : IClassFixture<CongeAppBrowserFixture>
{
    [Fact]
    public async Task La_page_d_accueil_est_accessible()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();

        var response = await page.GotoAsync(CongeAppBrowserFixture.AppUrl);

        Assert.NotNull(response);
        Assert.True(response!.Ok, $"Expected a successful response but got {response.Status}.");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Equal("Gestion des congés", await page.TitleAsync());
    }

    [Fact]
    public async Task Le_formulaire_de_conge_est_visible()
    {
        var page = await AuthenticatedPageAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = "Jean Dupont" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Poser un congé" })).ToBeVisibleAsync();
        await Expect(page.GetByLabel("Date de début")).ToBeVisibleAsync();
        await Expect(page.GetByLabel("Date de fin")).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Poser le congé" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Une_demande_de_conge_valide_est_acceptee()
    {
        var page = await AuthenticatedPageAsync();
        await OpenUserAsync(page);

        await FillDatesAsync(page, "2026-10-05", "2026-10-06");
        await page.GetByRole(AriaRole.Button, new() { Name = "Poser le congé" }).ClickAsync();

        await Expect(page.GetByText("05/10/2026 – 06/10/2026")).ToBeVisibleAsync();
        await Expect(page.GetByText("2 jour(s) ouvré(s)")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Les_champs_obligatoires_sont_valides()
    {
        var page = await AuthenticatedPageAsync();
        await OpenUserAsync(page);

        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Poser le congé" })).ToBeDisabledAsync();
        await Expect(page.GetByText("Choisissez une période contenant au moins un jour ouvré.")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Une_date_de_fin_anterieure_a_la_date_de_debut_est_refusee()
    {
        var page = await AuthenticatedPageAsync();
        await OpenUserAsync(page);

        await FillDatesAsync(page, "2026-10-06", "2026-10-05");

        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Poser le congé" })).ToBeDisabledAsync();
        await Expect(page.GetByText("Choisissez une période contenant au moins un jour ouvré.")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Une_duree_de_conge_invalide_est_refusee()
    {
        var page = await AuthenticatedPageAsync();
        await OpenUserAsync(page);

        await FillDatesAsync(page, "2026-10-10", "2026-10-11");

        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Poser le congé" })).ToBeDisabledAsync();
        await Expect(page.GetByText("Choisissez une période contenant au moins un jour ouvré.")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Les_donnees_saisies_sont_conservees_apres_validation()
    {
        var page = await AuthenticatedPageAsync();
        await OpenUserAsync(page);
        await FillDatesAsync(page, "2026-10-12", "2026-10-13");
        await page.GetByRole(AriaRole.Button, new() { Name = "Poser le congé" }).ClickAsync();

        await page.ReloadAsync();

        await Expect(page.GetByText("12/10/2026 – 13/10/2026")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task La_page_ne_genere_pas_d_erreur_javascript()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var errors = new List<string>();
        var page = await context.NewPageAsync();
        page.PageError += (_, error) => errors.Add(error);

        await AuthenticateAsync(page);
        await OpenUserAsync(page);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Les_liens_et_boutons_principaux_fonctionnent()
    {
        var page = await AuthenticatedPageAsync();
        await OpenUserAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Tous les utilisateurs" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Les utilisateurs" })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Jean Dupont" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Le_parcours_principal_fonctionne_sur_une_vue_mobile()
    {
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 390, Height = 844 }
        });
        var page = await context.NewPageAsync();
        await AuthenticateAsync(page);

        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Les utilisateurs" })).ToBeVisibleAsync();
        await OpenUserAsync(page);
        await Expect(page.GetByLabel("Date de début")).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Poser le congé" })).ToBeVisibleAsync();
    }

    private async Task<IPage> AuthenticatedPageAsync()
    {
        var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await AuthenticateAsync(page);
        return page;
    }

    private static async Task AuthenticateAsync(IPage page)
    {
        await page.GotoAsync(CongeAppBrowserFixture.AppUrl);
        await page.GetByLabel("Mot de passe").FillAsync("1234");
        await page.GetByRole(AriaRole.Button, new() { Name = "Se connecter" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Les utilisateurs" })).ToBeVisibleAsync();
    }

    private static async Task OpenUserAsync(IPage page)
    {
        await page.GetByRole(AriaRole.Link, new() { Name = "Jean Dupont" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Poser un congé" })).ToBeVisibleAsync();
    }

    private static async Task FillDatesAsync(IPage page, string start, string end)
    {
        await page.GetByLabel("Date de début").FillAsync(start);
        await page.GetByLabel("Date de fin").FillAsync(end);
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
