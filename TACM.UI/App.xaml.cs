using TACM.UI.Pages;
using TACM.Data;
using TACM.UI.Utils;

namespace TACM.UI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        // Note: Window maximization moved to CreateWindow to avoid startup issues
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = new AppShell
        {
            FlyoutBehavior = FlyoutBehavior.Disabled
        };

        var window = new Window(appShell);
        
#if WINDOWS
        // Maximize window after creation
        window.Created += (s, e) =>
        {
            var platformWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (platformWindow != null)
            {
                try
                {
                    TACM.UI.Platforms.Windows.WindowExtensions.Maximize(platformWindow);
                }
                catch (Exception ex)
                {
                    AppLogger.Log($"Failed to maximize window: {ex.Message}");
                }
            }
        };
#endif

        return window;
    }

    protected override void OnStart()
    {
        base.OnStart();
        
        // Initialize database on app start - fire and forget
        Task.Run(async () => await InitializeDatabaseAsync());
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            // Add a small delay to ensure app is fully initialized
            await Task.Delay(100);
            
            AppLogger.Log("Starting database initialization...");
            
            // Log diagnostic information
            DatabaseDiagnostics.LogDiagnostics();
            
            using var context = TacmDbContextFactory.CreateDbContext();
            
            // Ensure database and tables are created
            bool wasCreated = context.Database.EnsureCreated();
            
            AppLogger.Log($"Database initialized successfully (wasCreated: {wasCreated})");
            
            // Seed default settings if database was just created or no settings exist
            if (!context.Settings.Any())
            {
                AppLogger.Log("No settings found, seeding default settings...");
                
                var defaultSettings = new Entities.Settings
                {
                    Id = 0,
                    Target = 'X',
                    FontSize = 72,
                    T1 = 2500,
                    T2 = 1500,
                    T3 = 3500,
                    T4 = 3500,
                    GoProbability = 20,
                    NoProbability = 80,
                    Trials = 50,
                    RND = 3500,
                    AppTitle = "Test of Verbal, Non Verbal",
                    VerbalMemoryTestDemoWordsQuantity = 10,
                    VerbalMemoryTestWordsQuantity = 25,
                    NonVerbalMemoryTestDemoWordsQuantity = 10,
                    NonVerbalMemoryTestWordsQuantity = 25,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                
                context.Settings.Add(defaultSettings);
                context.SaveChanges();
                
                AppLogger.Log("Default settings seeded successfully");
            }
            else
            {
                AppLogger.Log("Settings already exist, skipping seed");
            }
        }
        catch (Exception ex)
        {
            AppLogger.Log($"CRITICAL: Database initialization failed - {ex.Message}");
            AppLogger.Log($"Stack trace: {ex.StackTrace}");
            
            // Show error to user
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Database Error",
                        $"Failed to initialize database. The app may not function correctly.\n\nError: {ex.Message}",
                        "OK"
                    );
                }
            });
        }
    }
}