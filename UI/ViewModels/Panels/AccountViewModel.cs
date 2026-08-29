using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;
using NMSE.IO;

namespace NMSE.UI.ViewModels.Panels;

public partial class RewardRowViewModel : ObservableObject
{
    [ObservableProperty] private string _rewardId = "";
    [ObservableProperty] private string _rewardName = "";

    /// <summary>Unlocked on the account, which is what accountdata.hg records.</summary>
    [ObservableProperty] private bool _isUnlocked;

    /// <summary>Redeemed in this save. Platform rewards have no per-save state.</summary>
    [ObservableProperty] private bool _isRedeemed;

    /// <summary>The expedition a season reward belongs to, blank when not applicable.</summary>
    public string Expedition { get; }

    public RewardRowViewModel(string id, string name, bool unlocked, bool redeemed = false, int seasonId = -1)
    {
        _rewardId = id;
        _rewardName = name;
        _isUnlocked = unlocked;
        _isRedeemed = redeemed;
        Expedition = seasonId >= 0 ? seasonId.ToString(CultureInfo.CurrentCulture) : "";
    }
}

public partial class AccountViewModel : PanelViewModelBase
{
    private JsonObject? _accountData;
    private string? _accountFilePath;
    private GameItemDatabase? _database;

    /// <summary>The loaded save, needed by the consistency check.</summary>
    private JsonObject? _saveData;
    private string? _saveDirectory;
    private bool _rewardsDbLoaded;

    private readonly List<AccountLogic.RewardDbEntry> _seasonRewardsDb = new();
    private readonly List<AccountLogic.RewardDbEntry> _twitchRewardsDb = new();
    private readonly List<AccountLogic.RewardDbEntry> _platformRewardsDb = new();

    /// <summary>
    /// Reward id to the game item it hands over. The Known* arrays store product ids, so
    /// a Twitch reward has to be resolved through this before it is written.
    /// </summary>
    private readonly Dictionary<string, string> _productIdMap = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Redeemed state as it was when the save loaded. Only rewards the user actually
    /// toggled get their Known* entries touched, so state a player keeps out of sync on
    /// purpose survives a round trip.
    /// </summary>
    private HashSet<string> _originalSeasonRedeemed = new(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> _originalTwitchRedeemed = new(StringComparer.OrdinalIgnoreCase);

    private SaveFileManager.Platform _currentPlatform = SaveFileManager.Platform.Steam;

    /// <summary>
    /// Only the PC platforms keep platform rewards in GCUSERSETTINGSDATA.MXML; consoles
    /// carry them in the account blob alone.
    /// </summary>
    private bool UsesMxml => _currentPlatform is SaveFileManager.Platform.Steam
        or SaveFileManager.Platform.GOG or SaveFileManager.Platform.Unknown;

    [ObservableProperty] private bool _showMxml;
    [ObservableProperty] private string _mxmlPath = "";
    [ObservableProperty] private string _mxmlStatus = "";

    [ObservableProperty] private string _statusText = UiStrings.Get("account.status_not_loaded");
    [ObservableProperty] private ObservableCollection<RewardRowViewModel> _seasonRewards = new();
    [ObservableProperty] private ObservableCollection<RewardRowViewModel> _twitchRewards = new();
    [ObservableProperty] private ObservableCollection<RewardRowViewModel> _platformRewards = new();

    [ObservableProperty] private string _seasonFilter = "";
    [ObservableProperty] private string _twitchFilter = "";
    [ObservableProperty] private string _platformFilter = "";

    [ObservableProperty] private ObservableCollection<RewardRowViewModel> _filteredSeasonRewards = new();
    [ObservableProperty] private ObservableCollection<RewardRowViewModel> _filteredTwitchRewards = new();
    [ObservableProperty] private ObservableCollection<RewardRowViewModel> _filteredPlatformRewards = new();

    public JsonObject? AccountData => _accountData;
    public string? AccountFilePath => _accountFilePath;

    public void SetDatabase(GameItemDatabase db) => _database = db;
    public void SetSaveDirectory(string? dir) => _saveDirectory = dir;

    public void LoadRewardsDatabase(string? jsonDirectory = null)
    {
        _seasonRewardsDb.Clear();
        _twitchRewardsDb.Clear();
        _platformRewardsDb.Clear();

        if (!string.IsNullOrEmpty(jsonDirectory))
            RewardDatabase.LoadFromJsonDirectory(jsonDirectory);

        _productIdMap.Clear();

        foreach (var reward in RewardDatabase.SeasonRewards)
            _seasonRewardsDb.Add(ToDbEntry(reward));
        foreach (var reward in RewardDatabase.TwitchRewards)
            _twitchRewardsDb.Add(ToDbEntry(reward));
        foreach (var reward in RewardDatabase.PlatformRewards)
            _platformRewardsDb.Add(ToDbEntry(reward));

        _rewardsDbLoaded = true;
    }

    private AccountLogic.RewardDbEntry ToDbEntry(RewardEntry reward)
    {
        if (!string.IsNullOrEmpty(reward.ProductId))
            _productIdMap[reward.Id] = reward.ProductId;

        return new AccountLogic.RewardDbEntry
        {
            Id = reward.Id,
            Name = ResolveDisplayName(reward),
            SeasonId = reward.SeasonId,
            StageId = reward.StageId,
            MustBeUnlocked = reward.Unlock,
        };
    }

    private string ResolveDisplayName(RewardEntry reward)
    {
        if (_database != null && !string.IsNullOrEmpty(reward.ProductId))
        {
            var item = _database.GetItem(reward.ProductId);
            if (item != null && !string.IsNullOrEmpty(item.Name))
                return item.Name;
        }
        return reward.Name;
    }

    public void LoadAccountFile(string saveDirectory)
    {
        SeasonRewards.Clear();
        TwitchRewards.Clear();
        PlatformRewards.Clear();
        _accountData = null;
        _accountFilePath = null;

        var data = AccountLogic.LoadAccountData(saveDirectory);
        if (data.ErrorMessage != null)
        {
            StatusText = data.ErrorMessage;
            return;
        }

        _accountData = data.AccountObject;
        _accountFilePath = data.AccountFilePath;

        // Platform rewards on PC live in GCUSERSETTINGSDATA.MXML as well as the account
        // blob, so those two have to be reconciled before the rows are built.
        var platformUnlocked = data.PlatformUnlocked;
        ShowMxml = UsesMxml;
        if (UsesMxml)
        {
            if (string.IsNullOrEmpty(MxmlPath))
            {
                var detected = MxmlRewardEditor.AutoDetectMxmlPath();
                if (detected != null) SetMxmlPath(detected);
            }

            if (!string.IsNullOrEmpty(MxmlPath))
            {
                var mxmlRewards = MxmlRewardEditor.ReadUnlockedRewards(MxmlPath);
                platformUnlocked = new HashSet<string>(
                    platformUnlocked.Where(mxmlRewards.Contains), StringComparer.OrdinalIgnoreCase);
            }
        }
        else
        {
            MxmlPath = "";
            MxmlStatus = "";
        }

        PopulateRewardList(SeasonRewards, _seasonRewardsDb, data.SeasonUnlocked);
        PopulateRewardList(TwitchRewards, _twitchRewardsDb, data.TwitchUnlocked);
        PopulateRewardList(PlatformRewards, _platformRewardsDb, platformUnlocked);

        ApplyRedeemedFromSave();
        ApplyFilters();
        StatusText = data.StatusMessage ?? "";
    }

    private static void PopulateRewardList(ObservableCollection<RewardRowViewModel> target,
        List<AccountLogic.RewardDbEntry> rewardsDb, HashSet<string> unlocked,
        HashSet<string>? redeemed = null)
    {
        target.Clear();
        foreach (var row in AccountLogic.BuildRewardRows(rewardsDb, unlocked, redeemed))
            target.Add(new RewardRowViewModel(row.Id, row.Name, row.Unlocked, row.Redeemed, row.SeasonId));
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _database = database;
        _saveData = saveData;

        if (!_rewardsDbLoaded)
        {
            string jsonDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "json");
            LoadRewardsDatabase(jsonDir);
        }

        if (!string.IsNullOrEmpty(_saveDirectory))
            LoadAccountFile(_saveDirectory);
        else
            ApplyRedeemedFromSave();
    }

    /// <summary>Which store the save came from, which decides whether MXML applies.</summary>
    public void SetPlatform(SaveFileManager.Platform platform)
    {
        _currentPlatform = platform;
        ShowMxml = UsesMxml;
    }

    private void SetMxmlPath(string path)
    {
        MxmlPath = path;
        MxmlStatus = UiStrings.Get(File.Exists(path) ? "account.file_found" : "account.file_not_found");
    }

    [RelayCommand]
    private async Task BrowseMxmlAsync()
    {
        if (OpenFilePickerFunc is null) return;
        string? path = await OpenFilePickerFunc(UiStrings.Get("common.browse"), ".MXML");
        if (!string.IsNullOrEmpty(path)) SetMxmlPath(path);
    }

    /// <summary>
    /// Reads the per-save redeemed sets into the grids, and snapshots them so only what
    /// the user changes is written back.
    /// </summary>
    private void ApplyRedeemedFromSave()
    {
        var (seasonRedeemed, twitchRedeemed) = AccountLogic.GetRedeemedSets(_saveData);

        _originalSeasonRedeemed = new HashSet<string>(seasonRedeemed, StringComparer.OrdinalIgnoreCase);
        _originalTwitchRedeemed = new HashSet<string>(twitchRedeemed, StringComparer.OrdinalIgnoreCase);

        foreach (var row in SeasonRewards) row.IsRedeemed = seasonRedeemed.Contains(row.RewardId);
        foreach (var row in TwitchRewards) row.IsRedeemed = twitchRedeemed.Contains(row.RewardId);

        // Platform rewards have no per-save redemption array.
        foreach (var row in PlatformRewards) row.IsRedeemed = false;
    }

    public override void SaveData(JsonObject saveData)
    {
        if (_accountData == null) return;

        var userSettings = _accountData.GetObject("UserSettingsData") ?? _accountData;

        var seasonRows = CollectRewardRows(SeasonRewards);
        var twitchRows = CollectRewardRows(TwitchRewards);
        var platformRows = CollectRewardRows(PlatformRewards);

        AccountLogic.SaveRewardList(
            seasonRows.Select(r => (r.Id, r.Unlocked)).ToList(), userSettings, "UnlockedSeasonRewards");
        AccountLogic.SaveRewardList(
            twitchRows.Select(r => (r.Id, r.Unlocked)).ToList(), userSettings, "UnlockedTwitchRewards");
        AccountLogic.SaveRewardList(
            platformRows.Select(r => (r.Id, r.Unlocked)).ToList(), userSettings, "UnlockedPlatformRewards");

        AccountLogic.SaveRedeemedRewards(saveData,
            seasonRows.Select(r => (r.Id, r.Redeemed)).ToList(),
            twitchRows.Select(r => (r.Id, r.Redeemed)).ToList(),
            _database);

        // Only rewards whose redeemed state the user actually changed get their Known*
        // entries touched. Anything left alone keeps whatever state the save had, which
        // a player may be holding out of sync deliberately.
        AccountLogic.SyncKnownArraysForChangedRewards(saveData,
            GetChangedRewards(seasonRows, _originalSeasonRedeemed), _database, _productIdMap);
        AccountLogic.SyncKnownArraysForChangedRewards(saveData,
            GetChangedRewards(twitchRows, _originalTwitchRedeemed), _database, _productIdMap);

        AccountLogic.CleanStaleKnownEntries(saveData,
            GetUnredeemedRows(seasonRows, _originalSeasonRedeemed),
            GetUnredeemedRows(twitchRows, _originalTwitchRedeemed),
            _database, _productIdMap);

        if (UsesMxml)
        {
            MxmlRewardEditor.SyncPlatformRewards(MxmlPath,
                platformRows.Select(r => (r.Id, r.Unlocked)).ToList());
        }
    }

    private static List<(string Id, bool Unlocked, bool Redeemed)> CollectRewardRows(
        ObservableCollection<RewardRowViewModel> rewards)
    {
        var result = new List<(string Id, bool Unlocked, bool Redeemed)>();
        foreach (var row in rewards)
            result.Add((row.RewardId, row.IsUnlocked, row.IsRedeemed));
        return result;
    }

    /// <summary>Rewards whose redeemed state differs from what the save held.</summary>
    private static List<(string Id, bool Redeemed)> GetChangedRewards(
        List<(string Id, bool Unlocked, bool Redeemed)> rows, HashSet<string> originalRedeemed)
        => [.. rows.Where(r => !string.IsNullOrEmpty(r.Id) && r.Redeemed != originalRedeemed.Contains(r.Id))
                   .Select(r => (r.Id, r.Redeemed))];

    /// <summary>
    /// Rewards the user turned off. Only these can leave a stale Known* entry behind.
    /// </summary>
    private static List<(string Id, bool Unlocked, bool Redeemed)> GetUnredeemedRows(
        List<(string Id, bool Unlocked, bool Redeemed)> rows, HashSet<string> originalRedeemed)
        => [.. rows.Where(r => !string.IsNullOrEmpty(r.Id) && !r.Redeemed && originalRedeemed.Contains(r.Id))];

    /// <summary>
    /// Reports rewards whose Redeemed* and Known* entries disagree, which is what leaves
    /// an unlocked reward the game will not hand over.
    /// </summary>
    [RelayCommand]
    private async Task CheckConsistencyAsync()
    {
        if (Dialogs is null) return;

        string title = UiStrings.Get("account.consistency_check");

        if (_saveData is null)
        {
            await Dialogs.ShowMessageAsync(title, UiStrings.Get("account.consistency_no_save"));
            return;
        }

        var issues = AccountLogic.CheckConsistencyStructured(_saveData, _database);
        if (issues.Count == 0)
        {
            await Dialogs.ShowMessageAsync(title, UiStrings.Get("account.consistency_ok"));
            return;
        }

        // Each line names the reward and the array its entry is missing from, which is
        // what a user needs in order to decide whether to fix it.
        var report = new System.Text.StringBuilder();
        foreach (var issue in issues.Take(40))
        {
            report.Append(string.IsNullOrEmpty(issue.Name) ? issue.Id : issue.Name)
                  .Append("  -  ")
                  .AppendLine(issue.Description);
        }
        if (issues.Count > 40)
            report.AppendLine("...");

        if (await Dialogs.ConfirmAsync(title,
                UiStrings.Format("account.consistency_found",
                    issues.Count.ToString(CultureInfo.CurrentCulture)) + "\n\n" + report,
                Services.DialogIcon.Warning))
        {
            AccountLogic.SyncKnownArraysForChangedRewards(_saveData,
                issues.Select(i => (i.Id, true)).ToList(), _database);
            await Dialogs.ShowMessageAsync(title, UiStrings.Get("account.consistency_ok"));
        }
    }

    [RelayCommand]
    private void UnlockAllSeason() => SetAll(FilteredSeasonRewards, true);

    [RelayCommand]
    private void LockAllSeason() => SetAll(FilteredSeasonRewards, false);

    [RelayCommand]
    private void UnlockAllTwitch() => SetAll(FilteredTwitchRewards, true);

    [RelayCommand]
    private void LockAllTwitch() => SetAll(FilteredTwitchRewards, false);

    [RelayCommand]
    private void UnlockAllPlatform() => SetAll(FilteredPlatformRewards, true);

    [RelayCommand]
    private void LockAllPlatform() => SetAll(FilteredPlatformRewards, false);

    [RelayCommand] private void RedeemAllSeason() => SetAllRedeemed(FilteredSeasonRewards, true);
    [RelayCommand] private void RemoveAllSeason() => SetAllRedeemed(FilteredSeasonRewards, false);
    [RelayCommand] private void RedeemAllTwitch() => SetAllRedeemed(FilteredTwitchRewards, true);
    [RelayCommand] private void RemoveAllTwitch() => SetAllRedeemed(FilteredTwitchRewards, false);
    [RelayCommand] private void RedeemAllPlatform() => SetAllRedeemed(FilteredPlatformRewards, true);
    [RelayCommand] private void RemoveAllPlatform() => SetAllRedeemed(FilteredPlatformRewards, false);

    // These act on the filtered view, so a filter narrows what a bulk button touches —
    // the same as the WinForms grids, which skipped hidden rows.
    private static void SetAll(ObservableCollection<RewardRowViewModel> rewards, bool value)
    {
        foreach (var row in rewards)
            row.IsUnlocked = value;
    }

    private static void SetAllRedeemed(ObservableCollection<RewardRowViewModel> rewards, bool value)
    {
        foreach (var row in rewards)
            row.IsRedeemed = value;
    }

    partial void OnSeasonFilterChanged(string value) => ApplyFilter(SeasonRewards, FilteredSeasonRewards, value);
    partial void OnTwitchFilterChanged(string value) => ApplyFilter(TwitchRewards, FilteredTwitchRewards, value);
    partial void OnPlatformFilterChanged(string value) => ApplyFilter(PlatformRewards, FilteredPlatformRewards, value);

    private void ApplyFilters()
    {
        ApplyFilter(SeasonRewards, FilteredSeasonRewards, SeasonFilter);
        ApplyFilter(TwitchRewards, FilteredTwitchRewards, TwitchFilter);
        ApplyFilter(PlatformRewards, FilteredPlatformRewards, PlatformFilter);
    }

    private static void ApplyFilter(ObservableCollection<RewardRowViewModel> source,
        ObservableCollection<RewardRowViewModel> target, string filterText)
    {
        target.Clear();
        foreach (var row in source)
        {
            if (string.IsNullOrWhiteSpace(filterText) ||
                row.RewardId.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
                row.RewardName.Contains(filterText, StringComparison.OrdinalIgnoreCase))
            {
                target.Add(row);
            }
        }
    }
}
