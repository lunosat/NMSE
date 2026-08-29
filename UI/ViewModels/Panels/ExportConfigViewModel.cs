using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

public partial class ConfigFieldViewModel : ObservableObject
{
    [ObservableProperty] private string _value = "";

    /// <summary>Stable identifier for the row, independent of the displayed language.</summary>
    public string Key { get; init; } = "";

    /// <summary>String-table key for the row's label.</summary>
    public string LabelKey { get; init; } = "";

    public string Label => string.IsNullOrEmpty(LabelKey) ? Key : UiStrings.Get(LabelKey);
}

public partial class ExportConfigViewModel : PanelViewModelBase
{
    public ObservableCollection<ConfigFieldViewModel> ExtensionFields { get; } = new();
    public ObservableCollection<ConfigFieldViewModel> TemplateFields { get; } = new();

    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private bool _isStatusSuccess;

    public string? ConfigFilePath { get; set; }

    private static readonly string[] ExtensionLabels =
    [
        "Exosuit", "Multi-tool", "Starship", "Corvette", "Corvette Snapshot",
        "Starship Cargo", "Starship Tech",
        "Freighter", "Freighter Cargo", "Freighter Tech",
        "Frigate", "Squadron",
        "Exocraft", "Exocraft Cargo", "Exocraft Tech",
        "Companion", "Base", "Chest", "Storage",
        "Discovery", "Settlement", "ByteBeat", "Outfit"
    ];

    private static readonly string[] TemplateLabels =
    [
        "Exosuit Cargo", "Exosuit Tech",
        "Multi-tool", "Starship", "Corvette", "Corvette Snapshot",
        "Starship Cargo", "Starship Tech",
        "Freighter", "Freighter Cargo", "Freighter Tech",
        "Frigate", "Squadron",
        "Exocraft", "Exocraft Cargo", "Exocraft Tech",
        "Companion", "Base", "Chest", "Storage",
        "Discovery", "Settlement", "ByteBeat", "Outfit"
    ];

    /// <summary>
    /// Help for the naming templates, assembled from the string table so it follows the
    /// selected language instead of being a fixed English block.
    /// </summary>
    public static string HelpText
    {
        get
        {
            string[] variableKeys =
            [
                "export_config.help_var_player_name", "export_config.help_var_ship_name",
                "export_config.help_var_multitool_name", "export_config.help_var_freighter_name",
                "export_config.help_var_frigate_name", "export_config.help_var_vehicle_name",
                "export_config.help_var_vehicle_type", "export_config.help_var_settlement_name",
                "export_config.help_var_base_name", "export_config.help_var_name",
                "export_config.help_var_type", "export_config.help_var_class",
                "export_config.help_var_seed", "export_config.help_var_rank",
                "export_config.help_var_race", "export_config.help_var_species",
                "export_config.help_var_creature_seed", "export_config.help_var_chest_number",
                "export_config.help_var_timestamp",
            ];

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(UiStrings.Get("export_config.help_heading")).AppendLine();
            foreach (string key in variableKeys)
                sb.Append("    ").AppendLine(UiStrings.Get(key));

            sb.AppendLine().AppendLine(UiStrings.Get("export_config.help_extensions_heading"));
            sb.AppendLine(UiStrings.Get("export_config.help_extensions_info"));
            return sb.ToString();
        }
    }

    public ExportConfigViewModel()
    {
        foreach (var key in ExtensionLabels)
            ExtensionFields.Add(new ConfigFieldViewModel { Key = key, LabelKey = LabelKeyFor(key) });

        foreach (var key in TemplateLabels)
            TemplateFields.Add(new ConfigFieldViewModel { Key = key, LabelKey = LabelKeyFor(key) });
    }

    /// <summary>Maps a row's stable key to its string-table label key.</summary>
    private static string LabelKeyFor(string key) => key switch
    {
        "Exosuit" => "export_config.template_exosuit",
        "Multi-tool" => "export_config.template_multitool",
        "Starship" => "export_config.template_starship",
        "Corvette" => "export_config.template_corvette",
        "Corvette Snapshot" => "export_config.template_corvette_snapshot",
        "Starship Cargo" => "export_config.template_starship_cargo",
        "Starship Tech" => "export_config.template_starship_tech",
        "Freighter" => "export_config.template_freighter",
        "Freighter Cargo" => "export_config.template_freighter_cargo",
        "Freighter Tech" => "export_config.template_freighter_tech",
        "Frigate" => "export_config.template_frigate",
        "Squadron" => "export_config.template_squadron",
        "Exocraft" => "export_config.template_exocraft",
        "Exocraft Cargo" => "export_config.template_exocraft_cargo",
        "Exocraft Tech" => "export_config.template_exocraft_tech",
        "Companion" => "export_config.template_companion",
        "Base" => "export_config.template_base",
        "Chest" => "export_config.template_chest",
        "Storage" => "export_config.template_storage",
        "Discovery" => "export_config.template_discovery",
        "Settlement" => "export_config.template_settlement",
        "ByteBeat" => "export_config.template_bytebeat",
        "Exosuit Cargo" => "export_config.template_exosuit_cargo",
        "Exosuit Tech" => "export_config.template_exosuit_tech",
        "Outfit" => "export_config.template_outfit",
        _ => "",
    };

    public void LoadConfig()
    {
        var cfg = ExportConfig.Instance;

        SetExt("Exosuit", cfg.ExosuitExt);
        SetExt("Multi-tool", cfg.MultitoolExt);
        SetExt("Starship", cfg.StarshipExt);
        SetExt("Corvette", cfg.CorvetteExt);
        SetExt("Corvette Snapshot", cfg.CorvetteSnapshotExt);
        SetExt("Starship Cargo", cfg.StarshipCargoExt);
        SetExt("Starship Tech", cfg.StarshipTechExt);
        SetExt("Freighter", cfg.FreighterExt);
        SetExt("Freighter Cargo", cfg.FreighterCargoExt);
        SetExt("Freighter Tech", cfg.FreighterTechExt);
        SetExt("Frigate", cfg.FrigateExt);
        SetExt("Squadron", cfg.SquadronExt);
        SetExt("Exocraft", cfg.ExocraftExt);
        SetExt("Exocraft Cargo", cfg.ExocraftCargoExt);
        SetExt("Exocraft Tech", cfg.ExocraftTechExt);
        SetExt("Companion", cfg.CompanionExt);
        SetExt("Base", cfg.BaseExt);
        SetExt("Chest", cfg.ChestExt);
        SetExt("Storage", cfg.StorageExt);
        SetExt("Discovery", cfg.DiscoveryExt);
        SetExt("Settlement", cfg.SettlementExt);
        SetExt("ByteBeat", cfg.ByteBeatExt);
        SetExt("Outfit", cfg.OutfitExt);

        SetTpl("Exosuit Cargo", cfg.ExosuitCargoTemplate);
        SetTpl("Exosuit Tech", cfg.ExosuitTechTemplate);
        SetTpl("Multi-tool", cfg.MultitoolTemplate);
        SetTpl("Starship", cfg.StarshipTemplate);
        SetTpl("Corvette", cfg.CorvetteTemplate);
        SetTpl("Corvette Snapshot", cfg.CorvetteSnapshotTemplate);
        SetTpl("Starship Cargo", cfg.StarshipCargoTemplate);
        SetTpl("Starship Tech", cfg.StarshipTechTemplate);
        SetTpl("Freighter", cfg.FreighterTemplate);
        SetTpl("Freighter Cargo", cfg.FreighterCargoTemplate);
        SetTpl("Freighter Tech", cfg.FreighterTechTemplate);
        SetTpl("Frigate", cfg.FrigateTemplate);
        SetTpl("Squadron", cfg.SquadronTemplate);
        SetTpl("Exocraft", cfg.ExocraftTemplate);
        SetTpl("Exocraft Cargo", cfg.ExocraftCargoTemplate);
        SetTpl("Exocraft Tech", cfg.ExocraftTechTemplate);
        SetTpl("Companion", cfg.CompanionTemplate);
        SetTpl("Base", cfg.BaseTemplate);
        SetTpl("Chest", cfg.ChestTemplate);
        SetTpl("Storage", cfg.StorageTemplate);
        SetTpl("Discovery", cfg.DiscoveryTemplate);
        SetTpl("Settlement", cfg.SettlementTemplate);
        SetTpl("ByteBeat", cfg.ByteBeatTemplate);
        SetTpl("Outfit", cfg.OutfitTemplate);
    }

    private void ApplyConfig()
    {
        var cfg = ExportConfig.Instance;

        cfg.ExosuitExt = GetExt("Exosuit", cfg.ExosuitExt);
        cfg.MultitoolExt = GetExt("Multi-tool", cfg.MultitoolExt);
        cfg.StarshipExt = GetExt("Starship", cfg.StarshipExt);
        cfg.CorvetteExt = GetExt("Corvette", cfg.CorvetteExt);
        cfg.CorvetteSnapshotExt = GetExt("Corvette Snapshot", cfg.CorvetteSnapshotExt);
        cfg.StarshipCargoExt = GetExt("Starship Cargo", cfg.StarshipCargoExt);
        cfg.StarshipTechExt = GetExt("Starship Tech", cfg.StarshipTechExt);
        cfg.FreighterExt = GetExt("Freighter", cfg.FreighterExt);
        cfg.FreighterCargoExt = GetExt("Freighter Cargo", cfg.FreighterCargoExt);
        cfg.FreighterTechExt = GetExt("Freighter Tech", cfg.FreighterTechExt);
        cfg.FrigateExt = GetExt("Frigate", cfg.FrigateExt);
        cfg.SquadronExt = GetExt("Squadron", cfg.SquadronExt);
        cfg.ExocraftExt = GetExt("Exocraft", cfg.ExocraftExt);
        cfg.ExocraftCargoExt = GetExt("Exocraft Cargo", cfg.ExocraftCargoExt);
        cfg.ExocraftTechExt = GetExt("Exocraft Tech", cfg.ExocraftTechExt);
        cfg.CompanionExt = GetExt("Companion", cfg.CompanionExt);
        cfg.BaseExt = GetExt("Base", cfg.BaseExt);
        cfg.ChestExt = GetExt("Chest", cfg.ChestExt);
        cfg.StorageExt = GetExt("Storage", cfg.StorageExt);
        cfg.DiscoveryExt = GetExt("Discovery", cfg.DiscoveryExt);
        cfg.SettlementExt = GetExt("Settlement", cfg.SettlementExt);
        cfg.ByteBeatExt = GetExt("ByteBeat", cfg.ByteBeatExt);
        cfg.OutfitExt = GetExt("Outfit", cfg.OutfitExt);

        cfg.ExosuitCargoTemplate = GetTpl("Exosuit Cargo", cfg.ExosuitCargoTemplate);
        cfg.ExosuitTechTemplate = GetTpl("Exosuit Tech", cfg.ExosuitTechTemplate);
        cfg.MultitoolTemplate = GetTpl("Multi-tool", cfg.MultitoolTemplate);
        cfg.StarshipTemplate = GetTpl("Starship", cfg.StarshipTemplate);
        cfg.CorvetteTemplate = GetTpl("Corvette", cfg.CorvetteTemplate);
        cfg.CorvetteSnapshotTemplate = GetTpl("Corvette Snapshot", cfg.CorvetteSnapshotTemplate);
        cfg.StarshipCargoTemplate = GetTpl("Starship Cargo", cfg.StarshipCargoTemplate);
        cfg.StarshipTechTemplate = GetTpl("Starship Tech", cfg.StarshipTechTemplate);
        cfg.FreighterTemplate = GetTpl("Freighter", cfg.FreighterTemplate);
        cfg.FreighterCargoTemplate = GetTpl("Freighter Cargo", cfg.FreighterCargoTemplate);
        cfg.FreighterTechTemplate = GetTpl("Freighter Tech", cfg.FreighterTechTemplate);
        cfg.FrigateTemplate = GetTpl("Frigate", cfg.FrigateTemplate);
        cfg.SquadronTemplate = GetTpl("Squadron", cfg.SquadronTemplate);
        cfg.ExocraftTemplate = GetTpl("Exocraft", cfg.ExocraftTemplate);
        cfg.ExocraftCargoTemplate = GetTpl("Exocraft Cargo", cfg.ExocraftCargoTemplate);
        cfg.ExocraftTechTemplate = GetTpl("Exocraft Tech", cfg.ExocraftTechTemplate);
        cfg.CompanionTemplate = GetTpl("Companion", cfg.CompanionTemplate);
        cfg.BaseTemplate = GetTpl("Base", cfg.BaseTemplate);
        cfg.ChestTemplate = GetTpl("Chest", cfg.ChestTemplate);
        cfg.StorageTemplate = GetTpl("Storage", cfg.StorageTemplate);
        cfg.DiscoveryTemplate = GetTpl("Discovery", cfg.DiscoveryTemplate);
        cfg.SettlementTemplate = GetTpl("Settlement", cfg.SettlementTemplate);
        cfg.ByteBeatTemplate = GetTpl("ByteBeat", cfg.ByteBeatTemplate);
        cfg.OutfitTemplate = GetTpl("Outfit", cfg.OutfitTemplate);
    }

    [RelayCommand]
    private void SaveSettings()
    {
        var warnings = ValidateExtensions();
        ApplyConfig();

        if (ConfigFilePath != null)
        {
            try
            {
                ExportConfig.Instance.SaveToFile(ConfigFilePath);
                IsStatusSuccess = true;
                StatusText = warnings.Count > 0
                    ? $"Settings saved. {string.Join(" ", warnings)}"
                    : "Settings saved.";
            }
            catch (Exception ex)
            {
                IsStatusSuccess = false;
                StatusText = $"Save failed: {ex.Message}";
            }
        }
        else
        {
            IsStatusSuccess = true;
            StatusText = "Settings applied (no save path configured).";
        }
    }

    [RelayCommand]
    private async Task ResetDefaultsAsync()
    {
        // Discarding every custom extension and template deserves a confirmation.
        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("export_config.reset_title"),
                UiStrings.Get("export_config.reset_confirm"), Services.DialogIcon.Warning))
            return;

        ExportConfig.SetInstance(new ExportConfig());
        LoadConfig();
        IsStatusSuccess = true;
        StatusText = UiStrings.Get("export_config.status_reset");
    }

    private List<string> ValidateExtensions()
    {
        var warnings = new List<string>();
        foreach (var field in ExtensionFields)
        {
            string val = field.Value.Trim();
            if (string.IsNullOrWhiteSpace(val)) continue;
            if (!val.StartsWith('.'))
            {
                field.Value = "." + val;
                warnings.Add($"{field.Label} extension was missing leading dot (auto-corrected).");
            }
        }
        return warnings;
    }

    private void SetExt(string key, string value)
    {
        var field = ExtensionFields.FirstOrDefault(f => f.Key == key);
        if (field != null) field.Value = value;
    }

    private string GetExt(string key, string fallback)
    {
        var field = ExtensionFields.FirstOrDefault(f => f.Key == key);
        return field != null && !string.IsNullOrWhiteSpace(field.Value) ? field.Value.Trim() : fallback;
    }

    private void SetTpl(string key, string value)
    {
        var field = TemplateFields.FirstOrDefault(f => f.Key == key);
        if (field != null) field.Value = value;
    }

    private string GetTpl(string key, string fallback)
    {
        var field = TemplateFields.FirstOrDefault(f => f.Key == key);
        return field != null && !string.IsNullOrWhiteSpace(field.Value) ? field.Value.Trim() : fallback;
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        LoadConfig();
    }

    public override void SaveData(JsonObject saveData) { }
}
