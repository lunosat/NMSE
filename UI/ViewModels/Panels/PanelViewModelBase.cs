using NMSE.Data;
using NMSE.Models;
using NMSE.UI.Services;

namespace NMSE.UI.ViewModels.Panels;

public abstract class PanelViewModelBase : ViewModelBase
{
    /// <summary>
    /// Modal dialogs. Assigned by the shell; panels that only read data may leave it
    /// null, so every use is null-checked rather than assumed.
    /// </summary>
    public IDialogService? Dialogs { get; set; }

    public Func<string, string, string, Task<string?>>? SaveFilePickerFunc { get; set; }
    public Func<string, string, Task<string?>>? OpenFilePickerFunc { get; set; }

    /// <summary>
    /// Inventory grids this panel owns. The shell walks them after a save loads to hand
    /// each one the player state its auto-stack destinations need, and the dialog
    /// service its prompts go through.
    /// </summary>
    public virtual IEnumerable<Controls.InventoryGridViewModel> Grids => [];

    public virtual void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager) { }
    public virtual void SaveData(JsonObject saveData) { }
}
