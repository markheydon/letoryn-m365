using Microsoft.Playwright;

namespace TenancyHub.E2E.Infrastructure;

internal sealed class BrowserSession : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;

    public static async Task<BrowserSession> CreateAsync(
        string? storageStatePath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var session = new BrowserSession();
        session._playwright = await Playwright.CreateAsync();
        session._browser = await session._playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });

        var contextOptions = new BrowserNewContextOptions();
        if (storageStatePath is not null)
        {
            contextOptions.StorageStatePath = storageStatePath;
        }

        session._context = await session._browser.NewContextAsync(contextOptions);
        return session;
    }

    public async Task<IPage> NewPageAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _context!.NewPageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_context is not null)
        {
            await _context.CloseAsync();
            _context = null;
        }

        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;
    }
}
