using NMSE.Core;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.Panels;

/// <summary>
/// Panel for basic browsing of game recipes (refining and cooking).
/// </summary>
public partial class RecipePanel : UserControl
{
    private RecipeDatabase? _recipeDb;
    private GameItemDatabase? _itemDb;
    private IconManager? _iconManager;

    public RecipePanel()
    {
        InitializeComponent();
        SetupLayout();
    }

    /// <summary>Sets the recipe and item databases for this panel.</summary>
    public void SetDatabases(RecipeDatabase? recipeDb, GameItemDatabase? itemDb)
    {
        _recipeDb = recipeDb;
        _itemDb = itemDb;
        PopulateGrid();
    }

    public void SetIconManager(IconManager? iconManager)
    {
        _iconManager = iconManager;
    }

    private void PopulateGrid()
    {
        _recipeGrid.Rows.Clear();
        if (_recipeDb == null) return;

        string filterType = _filterCombo.SelectedItem?.ToString() ?? "All";
        string search = _searchBox.Text.Trim();

        foreach (var recipe in _recipeDb.Recipes)
        {
            // Filter by type
            if (filterType == "Refining" && recipe.Cooking) continue;
            if (filterType == "Cooking" && !recipe.Cooking) continue;

            // Format display
            string inputs = string.Join(" + ", recipe.Ingredients.Select(i =>
            {
                string name = _itemDb?.GetItem(i.Id)?.Name ?? i.Id;
                return $"{i.Amount}x {name}";
            }));
            string output = recipe.Result != null
                ? $"{recipe.Result.Amount}x {(_itemDb?.GetItem(recipe.Result.Id)?.Name ?? recipe.Result.Id)}"
                : "";

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                bool match = inputs.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || output.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || recipe.RecipeName.Contains(search, StringComparison.OrdinalIgnoreCase);
                if (!match) continue;
            }

            int rowIdx = _recipeGrid.Rows.Add(inputs, "▶", output, recipe.TimeToMake, recipe.Cooking ? UiStrings.Get("recipe.type_cooking") : UiStrings.Get("recipe.type_refining"));
            _recipeGrid.Rows[rowIdx].Tag = recipe;
        }
    }

    private void OnFilterChanged(object? sender, EventArgs e)
    {
        PopulateGrid();
    }

    private void OnRecipeSelected(object? sender, EventArgs e)
    {
        if (_recipeGrid.SelectedRows.Count == 0)
        {
            _detailLabel.Text = UiStrings.Get("recipe.details_placeholder");
            return;
        }

        var recipe = _recipeGrid.SelectedRows[0].Tag as Recipe;
        if (recipe == null) return;

        var parts = new List<string>();
        if (!string.IsNullOrEmpty(recipe.RecipeName))
            parts.Add(UiStrings.Format("recipe.detail_recipe", recipe.RecipeName));
        parts.Add(UiStrings.Format("recipe.detail_type", recipe.Cooking ? UiStrings.Get("recipe.type_cooking") : UiStrings.Get("recipe.type_refining")));
        parts.Add(UiStrings.Format("recipe.detail_time", recipe.TimeToMake));

        if (recipe.Ingredients.Length > 0)
        {
            var ingNames = recipe.Ingredients.Select(i =>
            {
                string name = _itemDb?.GetItem(i.Id)?.Name ?? i.Id;
                return $"{i.Amount}x {name} ({i.Id})";
            });
            parts.Add(UiStrings.Format("recipe.detail_ingredients", string.Join(", ", ingNames)));
        }

        if (recipe.Result != null)
        {
            string resultName = _itemDb?.GetItem(recipe.Result.Id)?.Name ?? recipe.Result.Id;
            parts.Add(UiStrings.Format("recipe.detail_result", recipe.Result.Amount, resultName, recipe.Result.Id));
        }

        _detailLabel.Text = string.Join("  |  ", parts);
    }

    /// <summary>
    /// Custom paint handler for the Arrow column to render the ➡ emoji using GDI+
    /// (Graphics.DrawString) with Segoe UI Emoji font for color emoji rendering.
    /// </summary>
    private void OnRecipeGridCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        if (_recipeGrid.Columns[e.ColumnIndex].Name != "Arrow") return;

        e.PaintBackground(e.ClipBounds, e.State.HasFlag(DataGridViewElementStates.Selected));
        string text = e.Value?.ToString() ?? "";
        if (!string.IsNullOrEmpty(text) && e.Graphics != null)
        {
            using var emojiFont = new Font("Segoe UI Emoji", e.CellStyle?.Font?.Size ?? 9f);
            using var brush = new SolidBrush(e.CellStyle?.ForeColor ?? Color.Black);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(text, emojiFont, brush, e.CellBounds, sf);
        }
        e.Handled = true;
    }

    /// <summary>
    /// Repopulates the recipe grid so that localised item names are reflected.
    /// Called from OnLanguageSelected after ApplyLocalisation updates the GameItemDatabase.
    /// </summary>
    public void RefreshLanguage()
    {
        PopulateGrid();
    }

    public void ApplyUiLocalisation()
    {
        // Filter label
        _typeFilterLabel.Text = UiStrings.Get("recipe.type_filter");

        // Column headers
        if (_recipeGrid.Columns["Inputs"] is DataGridViewColumn inputs) inputs.HeaderText = UiStrings.Get("recipe.col_ingredients");
        if (_recipeGrid.Columns["Output"] is DataGridViewColumn output) output.HeaderText = UiStrings.Get("recipe.col_result");
        if (_recipeGrid.Columns["Time"] is DataGridViewColumn time) time.HeaderText = UiStrings.Get("recipe.col_time");
        if (_recipeGrid.Columns["Type"] is DataGridViewColumn type) type.HeaderText = UiStrings.Get("recipe.col_type");

        // NMS Recipes button
        if (_nmsRecipesBtn != null) _nmsRecipesBtn.Text = UiStrings.Get("recipe.nms_recipes_web");
    }
}
