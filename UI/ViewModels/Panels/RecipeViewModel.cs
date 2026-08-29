using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

public partial class RecipeRowViewModel : ObservableObject
{
    [ObservableProperty] private string _inputs = "";
    [ObservableProperty] private string _output = "";
    [ObservableProperty] private int _time;
    [ObservableProperty] private string _type = "";
    [ObservableProperty] private string _recipeName = "";

    public Recipe? Source { get; init; }
}

/// <summary>One entry of the player's learned refiner and cooking recipes.</summary>
public partial class KnownRecipeViewModel : ObservableObject
{
    /// <summary>The id as the save spells it, caret prefix included.</summary>
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";

    public override string ToString() => Name;
}

public partial class RecipeViewModel : PanelViewModelBase
{
    private RecipeDatabase? _recipeDb;
    private GameItemDatabase? _itemDb;

    [ObservableProperty] private ObservableCollection<RecipeRowViewModel> _recipes = new();
    [ObservableProperty] private RecipeRowViewModel? _selectedRecipe;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private int _selectedFilterIndex;
    [ObservableProperty] private string _detailText = UiStrings.Get("recipe.details_placeholder");

    [ObservableProperty] private string[] _filterOptions = BuildFilterOptions();

    private static string[] BuildFilterOptions() =>
    [
        UiStrings.Get("recipe.filter_all"),
        UiStrings.Get("recipe.type_refining"),
        UiStrings.Get("recipe.type_cooking"),
    ];

    public override void ApplyLocalisation()
    {
        FilterOptions = BuildFilterOptions();
        PopulateGrid();
    }

    partial void OnSearchTextChanged(string value) => PopulateGrid();
    partial void OnSelectedFilterIndexChanged(int value) => PopulateGrid();

    partial void OnSelectedRecipeChanged(RecipeRowViewModel? value)
    {
        if (value?.Source == null)
        {
            DetailText = UiStrings.Get("recipe.details_placeholder");
            return;
        }

        var recipe = value.Source;
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(recipe.RecipeName))
            parts.Add(UiStrings.Format("recipe.detail_recipe", recipe.RecipeName));

        parts.Add(UiStrings.Format("recipe.detail_type",
            UiStrings.Get(recipe.Cooking ? "recipe.type_cooking" : "recipe.type_refining")));
        parts.Add(UiStrings.Format("recipe.detail_time",
            recipe.TimeToMake.ToString(CultureInfo.CurrentCulture)));

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
            parts.Add(UiStrings.Format("recipe.detail_result",
                recipe.Result.Amount.ToString(CultureInfo.CurrentCulture), resultName, recipe.Result.Id));
        }

        DetailText = string.Join("  |  ", parts);
    }

    public void SetDatabases(RecipeDatabase? recipeDb, GameItemDatabase? itemDb)
    {
        _recipeDb = recipeDb;
        _itemDb = itemDb;
        PopulateGrid();
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _itemDb = database;
        _playerState = saveData.GetObject("PlayerStateData");
        LoadKnownRecipes();
    }

    // ================================ Known recipes =================================

    private JsonObject? _playerState;

    /// <summary>
    /// The recipes the player has actually learned, as opposed to the full database the
    /// info tab lists. These are what the game reads from KnownRefinerRecipes.
    /// </summary>
    public ObservableCollection<KnownRecipeViewModel> KnownRecipes { get; } = new();

    [ObservableProperty] private KnownRecipeViewModel? _selectedKnownRecipe;

    private void LoadKnownRecipes()
    {
        KnownRecipes.Clear();

        var known = _playerState?.GetArray("KnownRefinerRecipes");
        if (known is null) return;

        for (int i = 0; i < known.Length; i++)
        {
            string id = known.GetString(i) ?? "";
            if (string.IsNullOrEmpty(id) || id == "^") continue;

            string bare = id.TrimStart('^');
            KnownRecipes.Add(new KnownRecipeViewModel
            {
                Id = id,
                Name = _recipeDb?.GetRecipe(bare)?.RecipeName is { Length: > 0 } n ? n : bare,
            });
        }
    }

    /// <summary>
    /// Offers the recipes the player has not learned yet. Listing the ones they already
    /// have would only let them be added twice.
    /// </summary>
    [RelayCommand]
    private async Task AddKnownRecipeAsync()
    {
        if (Dialogs is null || _recipeDb is null) return;

        var have = new HashSet<string>(
            KnownRecipes.Select(r => r.Id.TrimStart('^')), StringComparer.OrdinalIgnoreCase);

        var missing = _recipeDb.Recipes
            .Where(r => !string.IsNullOrEmpty(r.Id) && !have.Contains(r.Id))
            .OrderBy(r => string.IsNullOrEmpty(r.RecipeName) ? r.Id : r.RecipeName)
            .ToList();

        if (missing.Count == 0) return;

        int? chosen = await Dialogs.ChooseAsync(
            UiStrings.Get("recipe.add_recipe_title"),
            UiStrings.Get("recipe.add_recipe"),
            missing.Select(r => string.IsNullOrEmpty(r.RecipeName) ? r.Id : r.RecipeName).ToList());
        if (chosen is null) return;

        var recipe = missing[chosen.Value];
        var array = _playerState?.GetArray("KnownRefinerRecipes");
        if (array is null) return;

        array.Add("^" + recipe.Id);
        LoadKnownRecipes();
    }

    [RelayCommand]
    private void RemoveKnownRecipe()
    {
        if (SelectedKnownRecipe is not { } row) return;

        var array = _playerState?.GetArray("KnownRefinerRecipes");
        if (array is null) return;

        for (int i = array.Length - 1; i >= 0; i--)
        {
            if (string.Equals(array.GetString(i), row.Id, StringComparison.OrdinalIgnoreCase))
            {
                array.RemoveAt(i);
                break;
            }
        }

        LoadKnownRecipes();
    }

    public override void SaveData(JsonObject saveData) { }

    private void PopulateGrid()
    {
        Recipes.Clear();
        SelectedRecipe = null;
        if (_recipeDb == null) return;

        string filterType = SelectedFilterIndex >= 0 && SelectedFilterIndex < FilterOptions.Length
            ? FilterOptions[SelectedFilterIndex]
            : "All";
        string search = SearchText?.Trim() ?? "";

        foreach (var recipe in _recipeDb.Recipes)
        {
            if (filterType == "Refining" && recipe.Cooking) continue;
            if (filterType == "Cooking" && !recipe.Cooking) continue;

            string inputs = string.Join(" + ", recipe.Ingredients.Select(i =>
            {
                string name = _itemDb?.GetItem(i.Id)?.Name ?? i.Id;
                return $"{i.Amount}x {name}";
            }));
            string output = recipe.Result != null
                ? $"{recipe.Result.Amount}x {(_itemDb?.GetItem(recipe.Result.Id)?.Name ?? recipe.Result.Id)}"
                : "";

            if (!string.IsNullOrEmpty(search))
            {
                bool match = inputs.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || output.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || recipe.RecipeName.Contains(search, StringComparison.OrdinalIgnoreCase);
                if (!match) continue;
            }

            Recipes.Add(new RecipeRowViewModel
            {
                Inputs = inputs,
                Output = output,
                Time = recipe.TimeToMake,
                Type = recipe.Cooking ? "Cooking" : "Refining",
                RecipeName = recipe.RecipeName,
                Source = recipe
            });
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = "";
        SelectedFilterIndex = 0;
    }

    [RelayCommand]
    private void OpenNmsRecipes()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://nomansskyrecipes.com/",
                UseShellExecute = true
            });
        }
        catch { }
    }

    [RelayCommand] private Task GoToRecipeJsonAsync() => GoToJsonAsync("PlayerStateData", "KnownRefinerRecipes");

}
