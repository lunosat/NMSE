namespace NMSE.UI.Services;

/// <summary>How a message dialog presents itself.</summary>
public enum DialogIcon { None, Information, Warning, Error, Question }

/// <summary>
/// Modal dialogs the panels need. The WinForms panels called MessageBox directly;
/// the view models cannot, so the shell supplies an implementation and every panel
/// asks through this.
/// </summary>
public interface IDialogService
{
    /// <summary>Shows a message and waits for acknowledgement.</summary>
    Task ShowMessageAsync(string title, string message, DialogIcon icon = DialogIcon.Information);

    /// <summary>
    /// Asks a yes/no question. Returns true when the user confirms.
    /// <paramref name="confirmLabel"/> names the accepting button when Yes reads wrong
    /// for the action — an import picker offers Import and Cancel, not Yes and No.
    /// </summary>
    Task<bool> ConfirmAsync(string title, string message, DialogIcon icon = DialogIcon.Question,
        string? confirmLabel = null);

    /// <summary>
    /// Asks for a single line of text. Returns null when cancelled, which callers must
    /// distinguish from an empty string the user deliberately entered.
    /// </summary>
    Task<string?> PromptAsync(string title, string prompt, string initialValue = "");

    /// <summary>Asks the user to pick one of <paramref name="options"/>. Null when cancelled.</summary>
    Task<int?> ChooseAsync(string title, string prompt, IReadOnlyList<string> options, int selectedIndex = 0);
}
