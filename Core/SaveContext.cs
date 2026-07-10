namespace NMSE.Core;

/// <summary>
/// Tracks the save-context (regular vs Expedition) for the currently loaded save.
/// Set in MainForm.LoadSaveData and reset in OnFormClosing.
/// All panel/Core helpers that touch root[BaseContext] should read this to decide
/// whether to use BaseContext or ExpeditionContext as the parent key.
/// </summary>
internal static class SaveContext
{
    public static bool IsExpeditionSave { get; private set; }

    public static void SetExpedition(bool isExpedition) => IsExpeditionSave = isExpedition;
    public static void Reset() => IsExpeditionSave = false;
}
