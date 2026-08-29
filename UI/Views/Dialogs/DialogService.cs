using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NMSE.Data;
using NMSE.UI.Services;

namespace NMSE.UI.Views.Dialogs;

/// <summary>
/// Builds the modal dialogs in code rather than AXAML: they are small, uniform, and
/// this keeps a window class from existing per message.
/// </summary>
public sealed class DialogService : IDialogService
{
    private readonly Func<Window?> _ownerProvider;

    public DialogService(Func<Window?> ownerProvider) => _ownerProvider = ownerProvider;

    public Task ShowMessageAsync(string title, string message, DialogIcon icon = DialogIcon.Information)
        => ShowAsync<object?>(title, message, icon, buttons: Buttons.Ok, build: null);

    public async Task<bool> ConfirmAsync(string title, string message, DialogIcon icon = DialogIcon.Question,
        string? confirmLabel = null)
        => await ShowAsync<bool>(title, message, icon, Buttons.YesNo, null, confirmLabel) is true;

    public async Task<string?> PromptAsync(string title, string prompt, string initialValue = "")
    {
        TextBox? box = null;
        var result = await ShowAsync<string>(title, prompt, DialogIcon.None, Buttons.OkCancel, panel =>
        {
            box = new TextBox
            {
                Text = initialValue,
                Margin = new Avalonia.Thickness(0, 10, 0, 0),
                FontFamily = FontFamily.Parse("monospace"),
            };
            panel.Children.Add(box);
            return () => box.Text ?? "";
        });
        return result;
    }

    public async Task<int?> ChooseAsync(string title, string prompt,
        IReadOnlyList<string> options, int selectedIndex = 0)
    {
        ComboBox? combo = null;
        var result = await ShowAsync<int>(title, prompt, DialogIcon.None, Buttons.OkCancel, panel =>
        {
            combo = new ComboBox
            {
                ItemsSource = options,
                SelectedIndex = options.Count > 0 ? Math.Clamp(selectedIndex, 0, options.Count - 1) : -1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 10, 0, 0),
            };
            panel.Children.Add(combo);
            return () => combo.SelectedIndex;
        });
        return result;
    }

    private enum Buttons { Ok, OkCancel, YesNo }

    /// <summary>
    /// Assembles and shows the dialog. <paramref name="build"/> may add an input control
    /// and returns a delegate that reads its value when the user accepts.
    /// </summary>
    private async Task<T?> ShowAsync<T>(string title, string message, DialogIcon icon,
        Buttons buttons, Func<StackPanel, Func<T>>? build, string? confirmLabel = null)
    {
        var owner = _ownerProvider();

        var body = new StackPanel { Spacing = 0 };
        var readValue = build?.Invoke(PrepareBody(body, message, icon));

        T? result = default;
        bool accepted = false;

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Margin = new Avalonia.Thickness(0, 18, 0, 0),
        };

        var dialog = new Window
        {
            Title = title,
            MinWidth = 380,
            MaxWidth = 620,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = owner is null
                ? WindowStartupLocation.CenterScreen
                : WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false,
        };

        void Accept()
        {
            accepted = true;
            if (readValue is not null) result = readValue();
            dialog.Close();
        }

        switch (buttons)
        {
            case Buttons.Ok:
            {
                var ok = new Button { Content = UiStrings.Get("common.ok"), IsDefault = true, MinWidth = 88 };
                ok.Classes.Add("primary");
                ok.Click += (_, _) => Accept();
                actions.Children.Add(ok);
                break;
            }
            case Buttons.OkCancel:
            {
                var cancel = new Button { Content = UiStrings.Get("common.cancel"), IsCancel = true, MinWidth = 88 };
                var ok = new Button { Content = UiStrings.Get("common.ok"), IsDefault = true, MinWidth = 88 };
                ok.Classes.Add("primary");
                cancel.Click += (_, _) => dialog.Close();
                ok.Click += (_, _) => Accept();
                actions.Children.Add(cancel);
                actions.Children.Add(ok);
                break;
            }
            case Buttons.YesNo:
            {
                bool named = !string.IsNullOrEmpty(confirmLabel);
                var no = new Button
                {
                    Content = UiStrings.Get(named ? "common.cancel" : "common.no"),
                    IsCancel = true,
                    MinWidth = 88,
                };
                var yes = new Button
                {
                    Content = named ? confirmLabel! : UiStrings.Get("common.yes"),
                    IsDefault = true,
                    MinWidth = 88,
                };
                yes.Classes.Add("primary");
                no.Click += (_, _) => dialog.Close();
                yes.Click += (_, _) => { accepted = true; if (readValue is not null) result = readValue(); dialog.Close(); };
                actions.Children.Add(no);
                actions.Children.Add(yes);
                break;
            }
        }

        body.Children.Add(actions);
        dialog.Content = new Border { Padding = new Avalonia.Thickness(20), Child = body };

        if (owner is not null) await dialog.ShowDialog(owner);
        else dialog.Show();

        if (!accepted) return default;
        return buttons == Buttons.YesNo && readValue is null ? (T)(object)true : result;
    }

    /// <summary>Adds the icon and message row, and returns the panel further content goes into.</summary>
    private static StackPanel PrepareBody(StackPanel body, string message, DialogIcon icon)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };

        if (icon != DialogIcon.None)
        {
            row.Children.Add(new TextBlock
            {
                Text = icon switch
                {
                    DialogIcon.Warning => "⚠",
                    DialogIcon.Error => "⛔",
                    DialogIcon.Question => "?",
                    _ => "ℹ",
                },
                FontSize = 22,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = icon switch
                {
                    DialogIcon.Warning => Brushes.Goldenrod,
                    DialogIcon.Error => Brushes.IndianRed,
                    _ => Brushes.SteelBlue,
                },
            });
        }

        row.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 520,
            VerticalAlignment = VerticalAlignment.Center,
        });

        body.Children.Add(row);
        return body;
    }
}
