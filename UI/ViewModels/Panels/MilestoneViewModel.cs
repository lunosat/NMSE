using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

public partial class MilestoneStatField : ObservableObject
{
    [ObservableProperty] private string _label = "";
    [ObservableProperty] private int _value;
    [ObservableProperty] private string _rankText = "";

    public string StatId { get; init; } = "";

    /// <summary>Guild stats carry a rank derived from the value.</summary>
    public bool IsGuild { get; init; }

    partial void OnValueChanged(int value)
    {
        if (!IsGuild) return;

        int rank = MilestoneLogic.GetGuildRank(StatId, value);
        int max = MilestoneLogic.GetGuildMaxRank(StatId);
        int next = MilestoneLogic.GetGuildNextRankIn(StatId, value);

        RankText = next < 0
            ? UiStrings.Format("milestone.guild_rank_of_max", rank.ToString(CultureInfo.CurrentCulture),
                max.ToString(CultureInfo.CurrentCulture)) + " " + UiStrings.Get("milestone.guild_rank_max")
            : UiStrings.Format("milestone.guild_rank_of_max", rank.ToString(CultureInfo.CurrentCulture),
                max.ToString(CultureInfo.CurrentCulture)) + " " +
              UiStrings.Format("milestone.guild_promo_in", next.ToString("N0", CultureInfo.CurrentCulture));
    }
}

public partial class MilestoneSection : ObservableObject
{
    [ObservableProperty] private string _title = "";
    public ObservableCollection<MilestoneStatField> Fields { get; } = new();
}

public partial class MilestoneViewModel : PanelViewModelBase
{
    // Tab 1 is three columns; tab 2 is two rows of four, matching the WinForms layout
    // so a stat sits where a returning user expects to find it.
    public ObservableCollection<MilestoneSection> Tab1Column1 { get; } = new();
    public ObservableCollection<MilestoneSection> Tab1Column2 { get; } = new();
    public ObservableCollection<MilestoneSection> Tab1Column3 { get; } = new();
    public ObservableCollection<MilestoneSection> Tab2Row1Column1 { get; } = new();
    public ObservableCollection<MilestoneSection> Tab2Row1Column2 { get; } = new();
    public ObservableCollection<MilestoneSection> Tab2Row1Column3 { get; } = new();
    public ObservableCollection<MilestoneSection> Tab2Row1Column4 { get; } = new();
    public ObservableCollection<MilestoneSection> Tab2Row2Column1 { get; } = new();
    public ObservableCollection<MilestoneSection> Tab2Row2Column2 { get; } = new();
    public ObservableCollection<MilestoneSection> Tab2Row2Column3 { get; } = new();
    public ObservableCollection<MilestoneSection> Tab2Row2Column4 { get; } = new();

    private readonly Dictionary<string, MilestoneStatField> _fieldMap = new();

    public MilestoneViewModel()
    {
        BuildLayout();
    }

    /// <summary>Section the following AddField calls attach to.</summary>
    private MilestoneSection? _current;

    /// <summary>
    /// Starts a section. <paramref name="fallback"/> is the English title used when the
    /// key is absent from the string table, matching how the WinForms designer declared
    /// both a title and its key.
    /// </summary>
    private void BeginSection(ObservableCollection<MilestoneSection> column, string locKey, string fallback)
    {
        string title = string.IsNullOrEmpty(locKey) ? fallback : UiStrings.Get(locKey);
        if (title == locKey) title = fallback;   // key missing: fall back to the English title

        _current = new MilestoneSection { Title = title };
        column.Add(_current);
    }

    /// <summary>
    /// Adds a stat row to the current section. Guild stats also show the rank the value
    /// earns and how much is left to the next one.
    /// </summary>
    private void AddField(string locKey, string statId, bool guild)
    {
        if (_current is null) return;

        string label = UiStrings.Get(locKey);
        var field = new MilestoneStatField { Label = label, StatId = statId, IsGuild = guild };
        _current.Fields.Add(field);

        // Several factions reuse a label such as "Standing", so the stat id is the key.
        _fieldMap[statId] = field;
    }

    private void BuildLayout()
    {
        BeginSection(Tab1Column1, "milestone.section_milestones", "Milestones");
        AddField("milestone.on_foot_exploration", "^DIST_WALKED", guild: false);
        AddField("milestone.alien_encounters", "^ALIENS_MET", guild: false);
        AddField("milestone.words_collected", "^WORDS_LEARNT", guild: false);
        AddField("milestone.most_units_accrued", "^MONEY", guild: false);
        AddField("milestone.ships_destroyed", "^ENEMIES_KILLED", guild: false);
        AddField("milestone.sentinels_destroyed", "^SENTINEL_KILLS", guild: false);
        AddField("milestone.space_exploration", "^DIST_WARP", guild: false);
        AddField("milestone.planet_zoology_scanned", "^DISC_ALL_CREATU", guild: false);
        BeginSection(Tab1Column1, "milestone.section_alien_factions", "Alien Factions");
        BeginSection(Tab1Column1, "milestone.gek", "Gek");
        AddField("milestone.standing", "^TRA_STANDING", guild: false);
        AddField("milestone.missions", "^TDONE_MISSIONS", guild: false);
        AddField("milestone.systems_visited", "^TSEEN_SYSTEMS", guild: false);
        AddField("milestone.gek_met", "^TRA_MET", guild: false);
        BeginSection(Tab1Column1, "milestone.vykeen", "Vy'keen");
        AddField("milestone.standing", "^WAR_STANDING", guild: false);
        AddField("milestone.missions", "^WDONE_MISSIONS", guild: false);
        AddField("milestone.systems_visited", "^WSEEN_SYSTEMS", guild: false);
        AddField("milestone.vykeen_met", "^WAR_MET", guild: false);
        BeginSection(Tab1Column1, "milestone.korvax", "Korvax");
        AddField("milestone.standing", "^EXP_STANDING", guild: false);
        AddField("milestone.missions", "^EDONE_MISSIONS", guild: false);
        AddField("milestone.systems_visited", "^ESEEN_SYSTEMS", guild: false);
        AddField("milestone.korvax_met", "^EXP_MET", guild: false);
        BeginSection(Tab1Column1, "milestone.autophage", "Autophage");
        AddField("milestone.standing", "^BUI_STANDING", guild: false);
        AddField("milestone.missions", "^BDONE_MISSIONS", guild: false);
        AddField("milestone.autophage_met", "^BUI_MET", guild: false);

        BeginSection(Tab1Column2, "milestone.section_kills", "Kills");
        AddField("milestone.ammo_fired", "^AMMO_FIRED", guild: false);
        AddField("milestone.predators", "^PREDS_KILLED", guild: false);
        AddField("milestone.sentinel_drones", "^DRONES_KILLED", guild: false);
        AddField("milestone.sentinel_quads", "^QUADS_KILLED", guild: false);
        AddField("milestone.sentinel_walkers", "^WALKERS_KILLED", guild: false);
        AddField("milestone.police", "^POLICE_KILLED", guild: false);
        AddField("milestone.civilian_freighters", "^CIV_FREI_KILLS", guild: false);
        AddField("milestone.fish_killed", "^FISH_KILLS", guild: false);
        AddField("milestone.flora_killed", "^FLORA_KILLED", guild: false);
        AddField("milestone.grubs", "^GRUBS_KILLED", guild: false);
        AddField("milestone.jellyfish_boss_1", "^JELLYBOSS", guild: false);
        AddField("milestone.kills_in_mech", "^KILLS_IN_MECH", guild: false);
        AddField("milestone.mechs", "^MECHS_KILLED", guild: false);
        AddField("milestone.miniworms", "^MINIWORM_KILL", guild: false);
        AddField("milestone.pirate_freighters_destroyed", "^PIR_FREI_WINS", guild: false);
        AddField("milestone.queens", "^QUEENS_KILLED", guild: false);
        AddField("milestone.road_kill", "^ROAD_KILL", guild: false);
        AddField("milestone.jellyfish_boss_2", "^S20_JELLYBOSS", guild: false);
        AddField("milestone.sentinel_freighters", "^SENTFREI_KILLED", guild: false);
        AddField("milestone.spiders", "^SPIDERS_KILLED", guild: false);
        AddField("milestone.spookfiend_boss", "^SPOOKBOSS", guild: false);
        AddField("milestone.spookfiend_juice", "^SPOOK_JUICE", guild: false);
        AddField("milestone.spookfiends", "^SPOOK_KILLS", guild: false);
        AddField("milestone.stone_guardians", "^STONE_KILLS", guild: false);

        BeginSection(Tab1Column3, "milestone.section_guilds", "Guilds");

        BeginSection(Tab1Column3, "milestone.merchants_guild", "Merchants Guild");
        AddField("milestone.standing", "^TGUILD_STAND", guild: true);
        AddField("milestone.missions_completed", "^TGDONE_MISSIONS", guild: true);
        AddField("milestone.plants_farmed", "^PLANTS_PLANTED", guild: true);
        AddField("milestone.proc_prods", "^PROC_PRODS", guild: true);
        BeginSection(Tab1Column3, "milestone.mercenaries_guild", "Mercenaries Guild");
        AddField("milestone.standing", "^WGUILD_STAND", guild: true);
        AddField("milestone.missions_completed", "^WGDONE_MISSIONS", guild: true);
        AddField("milestone.pirates", "^PIRATES_KILLED", guild: true);
        AddField("milestone.fiends", "^FIENDS_KILLED", guild: true);

        BeginSection(Tab1Column3, "milestone.explorers_guild", "Explorers Guild");
        AddField("milestone.standing", "^EGUILD_STAND", guild: true);
        AddField("milestone.missions_completed", "^EGDONE_MISSIONS", guild: true);
        AddField("milestone.rare_creatures", "^RARE_SCANNED", guild: true);
        AddField("milestone.flora_discovered", "^DISC_FLORA", guild: true);
        BeginSection(Tab1Column3, "milestone.outlaws", "Outlaws");
        AddField("milestone.standing", "^PIRATE_STAND", guild: true);
        AddField("milestone.missions_completed", "^PIRATE_MISSIONS", guild: true);
        AddField("milestone.bounties", "^BOUNTIES", guild: true);
        AddField("milestone.traders_killed", "^TRADERS_KILLED", guild: true);
        AddField("milestone.smuggled_value", "^SMUGGLE_VALUE", guild: true);

        BeginSection(Tab2Row1Column1, "milestone.section_other", "Other Milestones / Stats");
        AddField("milestone.total_play_time", "^TIME", guild: false);
        AddField("milestone.play_sessions", "^PLAY_SESSIONS", guild: false);
        AddField("milestone.total_deaths", "^DEATHS", guild: false);
        AddField("milestone.longest_life", "^LONGEST_LIFE", guild: false);
        AddField("milestone.units_all_time", "^MONEY_EVER", guild: false);
        AddField("milestone.nanites", "^NANITES", guild: false);
        AddField("milestone.nanites_all_time", "^NANITES_EVER", guild: false);
        AddField("milestone.ships_bought", "^SHIPS_BOUGHT", guild: false);
        AddField("milestone.distance_jetpack", "^DIST_JETPACK", guild: false);
        AddField("milestone.distance_flying", "^DIST_FLY", guild: false);
        AddField("milestone.distance_exocraft", "^DIST_EXO", guild: false);
        AddField("milestone.distance_pulse", "^DIST_PULSE", guild: false);
        AddField("milestone.distance_submarine", "^DIST_SUB", guild: false);
        AddField("milestone.distance_in_space", "^DIST_SPACE", guild: false);
        AddField("milestone.planets_discovered", "^DISC_PLANETS", guild: false);
        AddField("milestone.systems_discovered", "^DISC_SYSTEMS", guild: false);
        AddField("milestone.creatures_discovered", "^DISC_CREATURES", guild: false);
        AddField("milestone.minerals_discovered", "^DISC_MINERALS", guild: false);
        AddField("milestone.waypoints_discovered", "^DISC_WAYPOINTS", guild: false);
        AddField("milestone.planets_visited", "^VISIT_PLANETS", guild: false);
        AddField("milestone.creatures_fed", "^CREATURES_FED", guild: false);
        AddField("milestone.creatures_killed", "^CREATURES_KILL", guild: false);
        AddField("milestone.extreme_survival", "^EXTREME_WALK", guild: false);
        AddField("milestone.pirate_missions_done", "^PDONE_MISSIONS", guild: false);
        AddField("milestone.pirate_systems_visited", "^PIRATE_SYSTEMS", guild: false);

        BeginSection(Tab2Row1Column2, "", "");
        AddField("milestone.storm_survival", "^STORM_WALK", guild: false);
        AddField("milestone.cave_exploration", "^CAVE_WALK", guild: false);
        AddField("milestone.time_in_space", "^SPACE_TIME", guild: false);
        AddField("milestone.space_battles", "^SPACE_BATTLES", guild: false);
        AddField("milestone.fish_caught", "^FISH_CAUGHT", guild: false);
        AddField("milestone.fish_released", "^FISH_RELEASED", guild: false);
        AddField("milestone.bones_found", "^BONES_FOUND", guild: false);
        AddField("milestone.fossils_made", "^FOS_MADE", guild: false);
        AddField("milestone.salvage_looted", "^SALVAGE_LOOTED", guild: false);
        AddField("milestone.ruins_looted", "^RUINS_LOOTED", guild: false);
        AddField("milestone.gifts_given", "^GIFTS_GIVEN", guild: false);
        AddField("milestone.parts_placed", "^PARTS_PLACED", guild: false);
        AddField("milestone.base_parts_got", "^BASEPARTS_GOT", guild: false);
        AddField("milestone.pets_adopted", "^PETS_ADOPTED", guild: false);
        AddField("milestone.photo_mode_used", "^PHOTO_MODE_USED", guild: false);
        AddField("milestone.portal_warps", "^PORTAL_WARPS", guild: false);
        AddField("milestone.items_teleported", "^ITEMS_TELEPRT", guild: false);
        AddField("milestone.abandoned_freighters", "^ABAND_FREIGHTER", guild: false);
        AddField("milestone.acrobat", "^ACROBAT", guild: false);
        AddField("milestone.props_analysed", "^ANALYSE_PROP", guild: false);
        AddField("milestone.app_sessions", "^APP_SESSIONS", guild: false);
        AddField("milestone.artifact_hints", "^ARTIFACT_HINTS", guild: false);
        AddField("milestone.asteroids_destroyed", "^ASTEROIDS", guild: false);
        AddField("milestone.pirate_missions_req", "^MISSION_PIRATES", guild: false);

        BeginSection(Tab2Row1Column3, "", "");
        AddField("milestone.atlas_loops", "^ATLAS_LOOPS", guild: false);
        AddField("milestone.basecamp_lore", "^BASECOMP_LORE", guild: false);
        AddField("milestone.corvette_parts", "^BIGGS_PART_GOT", guild: false);
        AddField("milestone.black_hole_walks", "^BLACKHOLE_WALKS", guild: false);
        AddField("milestone.black_hole_warps", "^BLACKHOLE_WARPS", guild: false);
        AddField("milestone.dice_games_lost", "^DICE_GAME_LOST", guild: false);
        AddField("milestone.dice_games_won", "^DICE_GAME_WON", guild: false);
        AddField("milestone.early_warps", "^EARLY_WARPS", guild: false);
        AddField("milestone.eggs_received", "^EGGS_GOT", guild: false);
        AddField("milestone.eggs_hatched", "^EGGS_HATCHED", guild: false);
        AddField("milestone.eggs_modified", "^EGGS_MODDED", guild: false);
        AddField("milestone.egg_pods", "^EGG_PODS", guild: false);
        AddField("milestone.excavated", "^EXCAVATED", guild: false);
        AddField("milestone.walked_in_toxic", "^EX_TOX_WALK", guild: false);
        AddField("milestone.fiend_eggs", "^FIEND_EGG", guild: false);
        AddField("milestone.boots_fished", "^FISH_BOOT", guild: false);
        AddField("milestone.fish_cash", "^FISH_CASH", guild: false);
        AddField("milestone.legendary_fish", "^FISH_LEGEND", guild: false);
        AddField("milestone.fish_trapped", "^FISH_TRAPPED", guild: false);
        AddField("milestone.pods_broken", "^FPODS_BROKEN", guild: false);
        AddField("milestone.frigates", "^FRIGATES", guild: false);
        AddField("milestone.gravitino_balls", "^GRAVBALLS", guild: false);
        AddField("milestone.gravity_grabs", "^GRAV_GRAB", guild: false);
        AddField("milestone.gravity_pushes", "^GRAV_PUSH", guild: false);
        AddField("milestone.pirate_mysteries", "^PIRATE_MYSTERY", guild: false);

        BeginSection(Tab2Row1Column4, "", "");
        AddField("milestone.grav_throws", "^GRAV_THROW", guild: false);
        AddField("milestone.weapon_repairs", "^GUNSLOTREPAIRS", guild: false);
        AddField("milestone.head_repairs", "^HEAD_REPAIRS", guild: false);
        AddField("milestone.junk_metal", "^JM", guild: false);
        AddField("milestone.junk_metal_banked", "^JM_BANKED", guild: false);
        AddField("milestone.settlement_judgements", "^JUDGEMENTS", guild: false);
        AddField("milestone.longest_life_ex", "^LONGEST_LIFE_EX", guild: false);
        AddField("milestone.meditation", "^MEDITATION", guild: false);
        AddField("milestone.npcs_rescued", "^NPCS_RESCUED", guild: false);
        AddField("milestone.plants_gathered", "^PLANTS_GATHERED", guild: false);
        AddField("milestone.police_summons", "^POLICE_SUMMON", guild: false);
        AddField("milestone.poop_collected", "^POOP_COLLECTED", guild: false);
        AddField("milestone.quicksilver_spent", "^QS_SPENT", guild: false);
        AddField("milestone.resources_extracted", "^RES_EXTRACTED", guild: false);
        AddField("milestone.space_pois", "^SPACE_POI", guild: false);
        AddField("milestone.space_walks", "^SPACE_WALK", guild: false);
        AddField("milestone.storm_crystals", "^STORM_CRYSTALS", guild: false);
        AddField("milestone.times_in_space", "^TIMES_IN_SPACE", guild: false);
        AddField("milestone.treasure_found", "^TREASURE_FOUND", guild: false);
        AddField("milestone.tunnelled_distance", "^TUNNELLED", guild: false);
        AddField("milestone.vr_grabs", "^VR_GRABS", guild: false);
        AddField("milestone.vr_inits", "^VR_INIT", guild: false);
        AddField("milestone.vr_snapturns", "^VR_SNAPTURNS", guild: false);
        AddField("milestone.pirate_freighters_seen", "^PIR_FREI_SEEN", guild: false);

        BeginSection(Tab2Row2Column1, "milestone.section_disc_planets", "Discoveries (Planets)");
        AddField("milestone.disc_abandoned", "^DISC_ABAND", guild: false);
        AddField("milestone.disc_cold", "^DISC_P_COLD", guild: false);
        AddField("milestone.disc_dead", "^DISC_P_DEAD", guild: false);
        AddField("milestone.disc_dust", "^DISC_P_DUST", guild: false);
        AddField("milestone.disc_gas", "^DISC_P_GAS", guild: false);
        AddField("milestone.disc_hot", "^DISC_P_HOT", guild: false);
        AddField("milestone.disc_lava", "^DISC_P_LAVA", guild: false);
        AddField("milestone.disc_lush", "^DISC_P_LUSH", guild: false);
        AddField("milestone.disc_radioactive", "^DISC_P_RAD", guild: false);
        AddField("milestone.disc_rgb", "^DISC_P_RGB", guild: false);
        AddField("milestone.disc_swamp", "^DISC_P_SWAMP", guild: false);
        AddField("milestone.disc_toxic", "^DISC_P_TOX", guild: false);
        AddField("milestone.disc_water", "^DISC_P_WATER", guild: false);
        AddField("milestone.disc_weird", "^DISC_P_WEIRD", guild: false);
        AddField("milestone.disc_rare_system", "^DISC_RARE_SYS", guild: false);
        AddField("milestone.visit_cold", "^VISIT_COLD", guild: false);
        AddField("milestone.visit_dead", "^VISIT_DEAD", guild: false);
        AddField("milestone.visit_dust", "^VISIT_DUST", guild: false);
        AddField("milestone.visit_gas", "^VISIT_GAS", guild: false);
        AddField("milestone.visit_hot", "^VISIT_HOT", guild: false);
        AddField("milestone.visit_lava", "^VISIT_LAVA", guild: false);
        AddField("milestone.visit_lush", "^VISIT_LUSH", guild: false);
        AddField("milestone.visit_radioactive", "^VISIT_RAD", guild: false);
        AddField("milestone.visit_rgb", "^VISIT_RGB", guild: false);
        AddField("milestone.visit_swamp", "^VISIT_SWAMP", guild: false);
        AddField("milestone.visit_toxic", "^VISIT_TOX", guild: false);
        AddField("milestone.visit_water", "^VISIT_WATER", guild: false);
        AddField("milestone.visit_weird", "^VISIT_WEIRD", guild: false);

        BeginSection(Tab2Row2Column2, "milestone.section_disc_creatures", "Discoveries (Creatures)");
        AddField("milestone.disc_cre_aggressive", "^DISC_CRE_AGGRO", guild: false);
        AddField("milestone.disc_cre_flying", "^DISC_CRE_AIR", guild: false);
        AddField("milestone.disc_cre_cave", "^DISC_CRE_CAVE", guild: false);
        AddField("milestone.disc_cre_dissonant", "^DISC_CRE_DISS", guild: false);
        AddField("milestone.disc_cre_land", "^DISC_CRE_LAND", guild: false);
        AddField("milestone.disc_cre_robot", "^DISC_CRE_ROBOT", guild: false);
        AddField("milestone.disc_cre_water", "^DISC_CRE_WATER", guild: false);
        AddField("milestone.disc_cre_weird", "^DISC_CRE_WEIRD", guild: false);
        AddField("milestone.disc_glowing_strider", "^DISC_STRIDERGLO", guild: false);

        BeginSection(Tab2Row2Column3, "milestone.section_multiplayer", "Multiplayer");
        AddField("milestone.mp_depots_done", "^MP_DEPOT_DONE", guild: false);
        AddField("milestone.mp_depots_hacked", "^MP_DEPOT_HACK", guild: false);
        AddField("milestone.mp_events", "^MP_EVENT_COUNT", guild: false);
        AddField("milestone.mp_fish", "^MP_FISH_COUNT", guild: false);
        AddField("milestone.mp_full_session_count", "^MP_FULL_COUNT", guild: false);
        AddField("milestone.mp_full_time_spent", "^MP_FULL_TIME", guild: false);
        AddField("milestone.mp_missions_accessed", "^MP_MIS_ACCESS", guild: false);
        AddField("milestone.mp_missions_started", "^MP_MIS_STARTED", guild: false);
        AddField("milestone.mp_orb_count", "^MP_ORB_COUNT", guild: false);
        AddField("milestone.mp_orb_time", "^MP_ORB_TIME", guild: false);
        AddField("milestone.mp_pirate_waves", "^MP_PIRATES_WAVE", guild: false);
        AddField("milestone.mp_planet_quest_markers", "^MP_PQ_RMARKER", guild: false);
        AddField("milestone.mp_planet_quest_stones", "^MP_PQ_WSTONES", guild: false);
        AddField("milestone.mp_rep_fails", "^MP_REP_FAILS", guild: false);
        AddField("milestone.mp_sessions", "^MP_SESSIONS", guild: false);
        AddField("milestone.nexus_missions", "^NEXUS_MISSIONS", guild: false);
        AddField("milestone.nexus_planet_quests", "^NEXUS_MISS_PQ", guild: false);
        AddField("milestone.nexus_qs_missions", "^NEXUS_MISS_QS", guild: false);
        AddField("milestone.nexus_standing", "^NEXUS_STAND", guild: false);

        BeginSection(Tab2Row2Column4, "milestone.section_pet_battles", "Pet Battles");
        AddField("milestone.pb_boss_wins", "^PB_BOSS_WINS", guild: false);
        AddField("milestone.pb_challenge_hall_wins", "^PB_CHALL_WINS", guild: false);
        AddField("milestone.pb_nexus", "^PB_D_NEXUS", guild: false);
        AddField("milestone.pb_losses", "^PB_LOSSES", guild: false);
        AddField("milestone.pb_maxed_pets", "^PB_PETS_MAXED", guild: false);
        AddField("milestone.pb_wins", "^PB_WINS", guild: false);
        AddField("milestone.pets_owned", "^PETS_OWNED", guild: false);
        AddField("milestone.pet_levels_spent", "^PET_LEVEL_SPENT", guild: false);
        BeginSection(Tab2Row2Column4, "milestone.section_travel", "Travel");
        AddField("milestone.dist_any_corvette", "^DIST_BIGGS", guild: false);
        AddField("milestone.dist_creature", "^DIST_CRE_RIDE", guild: false);
        AddField("milestone.dist_own_corvette", "^DIST_MY_BIGGS", guild: false);
        AddField("milestone.dist_other_corvette", "^DIST_OTH_BIGGS", guild: false);
        AddField("milestone.dist_flying_pet", "^DIST_PET_FLY", guild: false);
        AddField("milestone.dist_pet", "^DIST_PET_RIDE", guild: false);
        AddField("milestone.dist_swam", "^DIST_SWAM", guild: false);
        AddField("milestone.walked_in_cold", "^EX_COLD_WALK", guild: false);
        AddField("milestone.walked_in_heat", "^EX_HOT_WALK", guild: false);
        AddField("milestone.walked_in_radiation", "^EX_RAD_WALK", guild: false);
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        foreach (var field in _fieldMap.Values)
            field.Value = 0;

        var entries = MilestoneLogic.FindGlobalStats(saveData);
        if (entries == null) return;

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries.GetObject(i);
            if (entry == null) continue;
            string? id = entry.GetString("Id");
            if (id == null || !_fieldMap.TryGetValue(id, out var field)) continue;

            field.Value = MilestoneLogic.ReadStatEntryValue(entry);
        }
    }

    public override void SaveData(JsonObject saveData)
    {
        var entries = MilestoneLogic.FindGlobalStats(saveData);
        if (entries == null) return;

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries.GetObject(i);
            if (entry == null) continue;
            string? id = entry.GetString("Id");
            if (id == null || !_fieldMap.TryGetValue(id, out var field)) continue;

            MilestoneLogic.WriteStatEntryValue(entry, field.Value);
        }
    }

    [RelayCommand] private Task GoToStatsJsonAsync() => GoToJsonAsync("PlayerStateData", "Stats");

}
