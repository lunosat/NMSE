using NMSE.Models;
using System.Text.Json;

namespace NMSE.Data;

/// <summary>
/// Loads and manages recipe data from the Recipes.json database file.
/// </summary>
public class RecipeDatabase
{
    private readonly List<Recipe> _recipes = new();
    private readonly Dictionary<string, List<Recipe>> _byResult = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<Recipe>> _byIngredient = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (string RecipeName, string RecipeType)> _englishBackup = new();

    /// <summary>All loaded recipes.</summary>
    public IReadOnlyList<Recipe> Recipes => _recipes;

    /// <summary>
    /// Loads recipes from a Recipes.json file.
    /// Returns true if recipes were loaded successfully.
    /// </summary>
    public bool LoadFromFile(string jsonPath)
    {
        if (!File.Exists(jsonPath)) return false;

        try
        {
            var content = File.ReadAllBytes(jsonPath);
            using var doc = JsonDocument.Parse(content);

            if (doc.RootElement.ValueKind != JsonValueKind.Array) return false;

            foreach (var elem in doc.RootElement.EnumerateArray())
            {
                var recipe = new Recipe
                {
                    Id = elem.TryGetProperty("Id", out var idProp) ? idProp.GetString() ?? "" : "",
                    Category = elem.TryGetProperty("Category", out var catProp) ? catProp.GetString() ?? "" : "",
                    RecipeType = elem.TryGetProperty("RecipeType", out var typeProp) ? typeProp.GetString() ?? "" : "",
                    RecipeName = elem.TryGetProperty("RecipeName", out var nameProp) ? nameProp.GetString() ?? "" : "",
                    RecipeNameLocStr = elem.TryGetProperty("RecipeName_LocStr", out var rnlProp) ? rnlProp.GetString() : null,
                    RecipeTypeLocStr = elem.TryGetProperty("RecipeType_LocStr", out var rtlProp) ? rtlProp.GetString() : null,
                    TimeToMake = elem.TryGetProperty("TimeToMake", out var timeProp) && timeProp.TryGetInt32(out int time) ? time : 0,
                    Cooking = elem.TryGetProperty("Cooking", out var cookProp) && cookProp.ValueKind == JsonValueKind.True,
                };

                if (elem.TryGetProperty("Result", out var resultProp))
                    recipe.Result = ParseElement(resultProp);

                if (elem.TryGetProperty("Ingredients", out var ingProp) && ingProp.ValueKind == JsonValueKind.Array)
                {
                    var ingredients = new List<RecipeElement>();
                    foreach (var ingElem in ingProp.EnumerateArray())
                    {
                        var parsed = ParseElement(ingElem);
                        if (parsed != null) ingredients.Add(parsed);
                    }
                    recipe.Ingredients = ingredients.ToArray();
                }

                _recipes.Add(recipe);

                // Index by result
                if (recipe.Result != null && !string.IsNullOrEmpty(recipe.Result.Id))
                {
                    if (!_byResult.TryGetValue(recipe.Result.Id, out var list))
                    {
                        list = new List<Recipe>();
                        _byResult[recipe.Result.Id] = list;
                    }
                    list.Add(recipe);
                }

                // Index by each ingredient
                foreach (var ing in recipe.Ingredients)
                {
                    if (string.IsNullOrEmpty(ing.Id)) continue;
                    if (!_byIngredient.TryGetValue(ing.Id, out var list))
                    {
                        list = new List<Recipe>();
                        _byIngredient[ing.Id] = list;
                    }
                    list.Add(recipe);
                }
            }

            return _recipes.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Applies localisation to recipe display names using the specified service.
    /// </summary>
    public int ApplyLocalisation(LocalisationService service)
    {
        if (!service.IsActive) { RevertLocalisation(); return 0; }

        int count = 0;
        foreach (var recipe in _recipes)
        {
            bool changed = false;

            if (!_englishBackup.ContainsKey(recipe.Id))
                _englishBackup[recipe.Id] = (recipe.RecipeName, recipe.RecipeType);

            var backup = _englishBackup[recipe.Id];
            recipe.RecipeName = backup.RecipeName;
            recipe.RecipeType = backup.RecipeType;

            if (!string.IsNullOrEmpty(recipe.RecipeNameLocStr))
            {
                var loc = service.Lookup(recipe.RecipeNameLocStr);
                if (loc != null) { recipe.RecipeName = loc; changed = true; }
            }

            if (!string.IsNullOrEmpty(recipe.RecipeTypeLocStr))
            {
                var loc = service.Lookup(recipe.RecipeTypeLocStr);
                if (loc != null) { recipe.RecipeType = loc; changed = true; }
            }

            if (changed) count++;
        }
        return count;
    }

    /// <summary>
    /// Reverts all recipe display names to their original English values.
    /// </summary>
    public void RevertLocalisation()
    {
        foreach (var kvp in _englishBackup)
        {
            var recipe = _recipes.Find(r => r.Id == kvp.Key);
            if (recipe != null)
            {
                recipe.RecipeName = kvp.Value.RecipeName;
                recipe.RecipeType = kvp.Value.RecipeType;
            }
        }
        _englishBackup.Clear();
    }

    /// <summary>Gets all recipes that produce the given item ID.</summary>
    public IReadOnlyList<Recipe> GetRecipesForResult(string itemId) =>
        _byResult.TryGetValue(itemId, out var list) ? list : [];

    /// <summary>Gets all recipes that use the given item ID as an ingredient.</summary>
    public IReadOnlyList<Recipe> GetRecipesUsingIngredient(string itemId) =>
        _byIngredient.TryGetValue(itemId, out var list) ? list : [];

    /// <summary>Gets all refining recipes (non-cooking).</summary>
    public IEnumerable<Recipe> GetRefiningRecipes() =>
        _recipes.Where(r => !r.Cooking);

    /// <summary>Gets all cooking recipes.</summary>
    public IEnumerable<Recipe> GetCookingRecipes() =>
        _recipes.Where(r => r.Cooking);

    /// <summary>Gets a single recipe by its ID.</summary>
    public Recipe? GetRecipe(string id) =>
        _recipes.Find(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase));

    private static RecipeElement? ParseElement(JsonElement elem)
    {
        if (elem.ValueKind != JsonValueKind.Object) return null;
        return new RecipeElement
        {
            Id = elem.TryGetProperty("Id", out var idProp) ? idProp.GetString() ?? "" : "",
            Type = elem.TryGetProperty("Type", out var typeProp) ? typeProp.GetString() ?? "" : "",
            Amount = elem.TryGetProperty("Amount", out var amtProp) && amtProp.TryGetInt32(out int amt) ? amt : 0,
        };
    }
}
