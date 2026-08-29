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

    public virtual void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager) { }
    public virtual void SaveData(JsonObject saveData) { }
}
