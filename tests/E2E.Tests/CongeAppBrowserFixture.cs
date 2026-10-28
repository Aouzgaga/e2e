using Microsoft.Playwright;

namespace E2E.Tests;

public sealed class CongeAppBrowserFixture : IAsyncLifetime
{
    public const string AppUrl = "https://aouzgaga.github.io/formation-gh-api/";
    private IPlaywright? playwright;
    private IBrowser? browser;

    public IBrowser Browser => browser ?? throw new InvalidOperationException("The browser is not initialized.");

    public async Task InitializeAsync()
    {
        playwright = await Playwright.CreateAsync();
        browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task DisposeAsync()
    {
        if (browser is not null)
        {
            await browser.CloseAsync();
        }

        playwright?.Dispose();
    }
}
