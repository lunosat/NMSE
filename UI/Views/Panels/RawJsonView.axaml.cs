using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using NMSE.Data;
using NMSE.UI.ViewModels.Panels;
using System.Xml;

namespace NMSE.UI.Views.Panels;

public partial class RawJsonView : UserControl
{
    private RawJsonViewModel? _vm;
    private bool _suppressEditorSync;

    public RawJsonView()
    {
        InitializeComponent();
        JsonEditor.TextChanged += OnEditorTextChanged;
        ApplyJsonHighlighting();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_vm is not null)
        {
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
            _vm.NodeRevealRequested -= RevealNode;
            _vm.EditValueRequested -= PromptForValueAsync;
            _vm.FilePathRequested -= PromptForPathAsync;
        }

        _vm = DataContext as RawJsonViewModel;

        if (_vm is not null)
        {
            _vm.PropertyChanged += OnViewModelPropertyChanged;
            _vm.NodeRevealRequested += RevealNode;
            _vm.EditValueRequested += PromptForValueAsync;
            _vm.FilePathRequested += PromptForPathAsync;
            SyncEditorFromViewModel();
        }
    }

    // ------------------------------------------------------------- text view

    /// <summary>
    /// AvaloniaEdit's Text is a plain CLR property rather than a styled one, so the
    /// two directions are kept in step by hand instead of through a binding.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RawJsonViewModel.JsonText)) SyncEditorFromViewModel();
        else if (e.PropertyName == nameof(RawJsonViewModel.ClipboardText)) CopyToClipboard();
    }

    private void SyncEditorFromViewModel()
    {
        if (_vm is null || JsonEditor.Text == _vm.JsonText) return;
        _suppressEditorSync = true;
        JsonEditor.Text = _vm.JsonText;
        _suppressEditorSync = false;
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (_suppressEditorSync || _vm is null) return;
        _vm.JsonText = JsonEditor.Text;
    }

    /// <summary>
    /// Installs a JSON highlighting definition. AvaloniaEdit ships highlighting for
    /// several languages but not JSON, so the rules are defined here.
    /// </summary>
    private void ApplyJsonHighlighting()
    {
        const string xshd = """
            <SyntaxDefinition name="JSON" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">
              <Color name="Key"     foreground="#FFB458" />
              <Color name="String"  foreground="#9FD17E" />
              <Color name="Number"  foreground="#6FC3DF" />
              <Color name="Literal" foreground="#C08FE0" />
              <Color name="Punct"   foreground="#8A7E70" />
              <RuleSet>
                <!-- A quoted run followed by a colon is a key; any other quoted run is
                     a string value. Ordering matters: the key rule has to win. -->
                <Span color="Key" multiline="false">
                  <Begin>"(?=(\\\\.|[^"\\\\])*"\s*:)</Begin>
                  <End>"</End>
                  <RuleSet>
                    <Span begin="\\\\" end="." />
                  </RuleSet>
                </Span>
                <Span color="String" multiline="false">
                  <Begin>"</Begin>
                  <End>"</End>
                  <RuleSet>
                    <Span begin="\\\\" end="." />
                  </RuleSet>
                </Span>
                <Keywords color="Literal">
                  <Word>true</Word>
                  <Word>false</Word>
                  <Word>null</Word>
                </Keywords>
                <Rule color="Number">\b\-?\d+(\.\d+)?([eE][+\-]?\d+)?\b</Rule>
                <Rule color="Punct">[{}\[\],:]</Rule>
              </RuleSet>
            </SyntaxDefinition>
            """;

        try
        {
            using var reader = XmlReader.Create(new StringReader(xshd));
            JsonEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
        catch
        {
            // Highlighting is cosmetic; the editor stays usable as plain text.
        }
    }

    // ----------------------------------------------------------------- tree

    private void OnTreeDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (_vm?.EditValueCommand.CanExecute(null) == true)
            _vm.EditValueCommand.Execute(null);
    }

    /// <summary>Brings a node into view after a search hit or a "go to JSON" jump.</summary>
    private void RevealNode(JsonTreeNodeViewModel node)
    {
        // Give the tree a beat to realise the containers that were just expanded.
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var container = JsonTree.TreeContainerFromItem(node);
            (container as Control)?.BringIntoView();
        }, Avalonia.Threading.DispatcherPriority.Background);
    }

    // -------------------------------------------------------------- dialogs

    /// <summary>Asks the user for a new scalar value; returns null when cancelled.</summary>
    private async Task<string?> PromptForValueAsync(string key, string currentValue)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner is null) return null;

        var box = new TextBox
        {
            Text = currentValue,
            AcceptsReturn = false,
            Margin = new Avalonia.Thickness(0, 8, 0, 12),
            FontFamily = FontFamily.Parse("monospace"),
        };

        string? result = null;
        var ok = new Button { Content = UiStrings.Get("common.ok"), IsDefault = true };
        var cancel = new Button { Content = UiStrings.Get("common.cancel"), IsCancel = true };

        var dialog = new Window
        {
            Title = UiStrings.Format("raw_json.edit_value_title", key),
            Width = 560,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(16),
                Children =
                {
                    new TextBlock { Text = UiStrings.Format("raw_json.edit_value_prompt", key) },
                    box,
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Spacing = 8,
                        Children = { cancel, ok },
                    },
                },
            },
        };

        ok.Click += (_, _) => { result = box.Text; dialog.Close(); };
        cancel.Click += (_, _) => dialog.Close();

        box.SelectAll();
        box.Focus();
        await dialog.ShowDialog(owner);
        return result;
    }

    /// <summary>
    /// Opens the platform file chooser through the storage provider, which routes to
    /// the XDG portal on Linux so it works under Flatpak.
    /// </summary>
    private async Task<string?> PromptForPathAsync(bool saving, string suggestedName)
    {
        var top = TopLevel.GetTopLevel(this);
        if (top is null) return null;

        var jsonType = new FilePickerFileType("JSON")
        {
            Patterns = new[] { "*.json" },
            MimeTypes = new[] { "application/json" },
        };

        if (saving)
        {
            var file = await top.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = UiStrings.Get("raw_json.export"),
                    SuggestedFileName = suggestedName,
                    DefaultExtension = "json",
                    FileTypeChoices = new[] { jsonType },
                });
            return file?.TryGetLocalPath();
        }

        var files = await top.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = UiStrings.Get("raw_json.import"),
                AllowMultiple = false,
                FileTypeFilter = new[] { jsonType },
            });
        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    private async void CopyToClipboard()
    {
        if (_vm?.ClipboardText is not { } text) return;
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null) await clipboard.SetTextAsync(text);
    }
}
