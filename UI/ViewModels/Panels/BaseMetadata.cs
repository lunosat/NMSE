using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

/// <summary>
/// The metadata a base carries beyond its name and its objects: where it is, who owns
/// it, and the versions and flags the game stamps on it when it is uploaded.
/// </summary>
/// <remarks>
/// Several of these are read-only in the panel because the game derives them — the game
/// mode and difficulty come from the save, not from the base — but they are shown
/// because a base copied between saves carries them and they explain why it behaves the
/// way it does.
/// </remarks>
public partial class BaseMetadataViewModel : ObservableObject
{
    private JsonObject? _data;
    private bool _loading;

    // Editable
    [ObservableProperty] private double? _baseVersion;
    [ObservableProperty] private double? _originalBaseVersion;
    [ObservableProperty] private string _galacticAddress = "";
    [ObservableProperty] private string _rid = "";
    [ObservableProperty] private double? _userData;
    [ObservableProperty] private double? _lastUpdateTimestamp;
    [ObservableProperty] private bool _isReported;
    [ObservableProperty] private bool _isFeatured;
    [ObservableProperty] private string _autoPower = "";

    // Read-only: the game derives these
    [ObservableProperty] private string _baseType = "";
    [ObservableProperty] private string _gameMode = "";
    [ObservableProperty] private string _difficulty = "";
    [ObservableProperty] private string _platformToken = "";
    [ObservableProperty] private string _ownerLid = "";
    [ObservableProperty] private string _ownerUid = "";
    [ObservableProperty] private string _ownerUsn = "";
    [ObservableProperty] private string _ownerPtk = "";
    [ObservableProperty] private string _ownerTs = "";
    [ObservableProperty] private string _lastEditedById = "";
    [ObservableProperty] private string _lastEditedByUsername = "";

    // Vectors, edited component by component
    [ObservableProperty] private double? _positionX;
    [ObservableProperty] private double? _positionY;
    [ObservableProperty] private double? _positionZ;
    [ObservableProperty] private double? _forwardX;
    [ObservableProperty] private double? _forwardY;
    [ObservableProperty] private double? _forwardZ;
    [ObservableProperty] private double? _screenshotAtX;
    [ObservableProperty] private double? _screenshotAtY;
    [ObservableProperty] private double? _screenshotAtZ;
    [ObservableProperty] private double? _screenshotPosX;
    [ObservableProperty] private double? _screenshotPosY;
    [ObservableProperty] private double? _screenshotPosZ;

    /// <summary>Raised when the user edits a field, not when one is loaded.</summary>
    public event Action? Modified;

    public void Load(JsonObject? data)
    {
        _loading = true;
        try
        {
            _data = data;
            if (data is null) { Clear(); return; }

            BaseVersion = ReadDouble(data, "BaseVersion");
            OriginalBaseVersion = ReadDouble(data, "OriginalBaseVersion");
            GalacticAddress = ReadAddress(data);
            UserData = ReadDouble(data, "UserData");
            LastUpdateTimestamp = ReadDouble(data, "LastUpdateTimestamp");
            Rid = data.GetString("RID") ?? "";

            (PositionX, PositionY, PositionZ) = ReadVector(data, "Position");
            (ForwardX, ForwardY, ForwardZ) = ReadVector(data, "Forward");
            (ScreenshotAtX, ScreenshotAtY, ScreenshotAtZ) = ReadVector(data, "ScreenshotAt");
            (ScreenshotPosX, ScreenshotPosY, ScreenshotPosZ) = ReadVector(data, "ScreenshotPos");

            try { IsReported = data.GetBool("IsReported"); } catch { IsReported = false; }
            try { IsFeatured = data.GetBool("IsFeatured"); } catch { IsFeatured = false; }

            AutoPower = data.GetString("AutoPowerSetting.BaseAutoPowerSetting") ?? "";
            BaseType = data.GetString("BaseType.PersistentBaseTypes") ?? "";
            GameMode = data.GetString("GameMode.PresetGameMode") ?? "";
            Difficulty = data.GetString("Difficulty.DifficultyPreset.DifficultyPresetType") ?? "";
            PlatformToken = data.GetString("PlatformToken") ?? "";

            OwnerLid = data.GetString("Owner.LID") ?? "";
            OwnerUid = data.GetString("Owner.UID") ?? "";
            OwnerUsn = data.GetString("Owner.USN") ?? "";
            OwnerPtk = data.GetString("Owner.PTK") ?? "";
            OwnerTs = ReadDouble(data, "Owner.TS")?.ToString(CultureInfo.InvariantCulture) ?? "";

            LastEditedById = data.GetString("LastEditedById") ?? "";
            LastEditedByUsername = data.GetString("LastEditedByUsername") ?? "";
        }
        finally { _loading = false; }
    }

    private void Clear()
    {
        BaseVersion = OriginalBaseVersion = UserData = LastUpdateTimestamp = null;
        GalacticAddress = Rid = AutoPower = "";
        BaseType = GameMode = Difficulty = PlatformToken = "";
        OwnerLid = OwnerUid = OwnerUsn = OwnerPtk = OwnerTs = "";
        LastEditedById = LastEditedByUsername = "";
        IsReported = IsFeatured = false;

        PositionX = PositionY = PositionZ = null;
        ForwardX = ForwardY = ForwardZ = null;
        ScreenshotAtX = ScreenshotAtY = ScreenshotAtZ = null;
        ScreenshotPosX = ScreenshotPosY = ScreenshotPosZ = null;
    }

    /// <summary>
    /// Writes the editable fields back. Only these are written: the rest are values the
    /// game derives, and overwriting them would put the base out of step with its save.
    /// </summary>
    public void Save()
    {
        if (_data is not { } data) return;

        WriteDouble(data, "BaseVersion", BaseVersion);
        WriteDouble(data, "OriginalBaseVersion", OriginalBaseVersion);
        WriteDouble(data, "UserData", UserData);
        WriteDouble(data, "LastUpdateTimestamp", LastUpdateTimestamp);

        try { data.Set("GalacticAddress", GalacticAddress); } catch { }
        try { data.Set("RID", Rid); } catch { }
        try { data.Set("IsReported", IsReported); } catch { }
        try { data.Set("IsFeatured", IsFeatured); } catch { }

        try
        {
            // The setting sits inside its own object when the save has one.
            if (data.GetObject("AutoPowerSetting") is { } autoPower)
                autoPower.Set("BaseAutoPowerSetting", AutoPower);
            else
                data.Set("AutoPowerSetting.BaseAutoPowerSetting", AutoPower);
        }
        catch { }

        WriteVector(data, "Position", PositionX, PositionY, PositionZ);
        WriteVector(data, "Forward", ForwardX, ForwardY, ForwardZ);
        WriteVector(data, "ScreenshotAt", ScreenshotAtX, ScreenshotAtY, ScreenshotAtZ);
        WriteVector(data, "ScreenshotPos", ScreenshotPosX, ScreenshotPosY, ScreenshotPosZ);
    }

    /// <summary>
    /// The address, normalised so a save that stores it as a number and one that stores
    /// it as a hex string both read the same.
    /// </summary>
    private static string ReadAddress(JsonObject data)
    {
        try { return NMSE.Core.Utilities.CoordinateHelper.NormalizeGalacticAddress(data.Get("GalacticAddress")); }
        catch { return ""; }
    }

    private static double? ReadDouble(JsonObject data, string key)
    {
        try { return data.GetDouble(key); } catch { return null; }
    }

    private static void WriteDouble(JsonObject data, string key, double? value)
    {
        if (value is null) return;
        try { data.Set(key, value.Value); } catch { }
    }

    private static (double?, double?, double?) ReadVector(JsonObject data, string key)
    {
        try
        {
            var arr = data.GetArray(key);
            if (arr is null || arr.Length < 3) return (null, null, null);
            return (arr.GetDouble(0), arr.GetDouble(1), arr.GetDouble(2));
        }
        catch { return (null, null, null); }
    }

    private static void WriteVector(JsonObject data, string key, double? x, double? y, double? z)
    {
        if (x is null || y is null || z is null) return;
        try
        {
            var arr = data.GetArray(key);
            if (arr is null || arr.Length < 3) return;
            arr.Set(0, x.Value);
            arr.Set(1, y.Value);
            arr.Set(2, z.Value);
        }
        catch { }
    }

    // Any edit marks the save dirty, so the shell knows there is something to write.
    partial void OnBaseVersionChanged(double? value) => RaiseModified();
    partial void OnOriginalBaseVersionChanged(double? value) => RaiseModified();
    partial void OnGalacticAddressChanged(string value) => RaiseModified();
    partial void OnRidChanged(string value) => RaiseModified();
    partial void OnUserDataChanged(double? value) => RaiseModified();
    partial void OnLastUpdateTimestampChanged(double? value) => RaiseModified();
    partial void OnIsReportedChanged(bool value) => RaiseModified();
    partial void OnIsFeaturedChanged(bool value) => RaiseModified();
    partial void OnAutoPowerChanged(string value) => RaiseModified();

    private void RaiseModified()
    {
        if (!_loading) Modified?.Invoke();
    }
}
