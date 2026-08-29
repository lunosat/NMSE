using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using NMSE.Config;
using NMSE.UI.Views;

namespace NMSE;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var theme = AppConfig.Instance.Theme;
        if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
            RequestedThemeVariant = ThemeVariant.Light;
        else
            RequestedThemeVariant = ThemeVariant.Dark;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // View models read their fixed lists — difficulty presets, sort modes, race
            // names — through UiStrings when they are constructed. Loading the table
            // first means those reads find strings rather than the keys themselves.
            var config = NMSE.Config.AppConfig.Instance;
            config.Initialize();

            NMSE.Data.UiStrings.SetDirectory(
                System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "ui", "lang"));
            NMSE.Data.UiStrings.Load(config.Language);

            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void SetTheme(string theme)
    {
        if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
            RequestedThemeVariant = ThemeVariant.Light;
        else
            RequestedThemeVariant = ThemeVariant.Dark;

        AppConfig.Instance.Theme = theme;
        AppConfig.Instance.Save();
    }
}
