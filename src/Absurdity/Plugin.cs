using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace Absurdity;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static Plugin Instance { get; private set; } = null!;
    internal static ManualLogSource Log { get; private set; } = null!;

    internal static ConfigEntry<bool> ShoreTornadoes = null!;
    internal static ConfigEntry<bool> EveryLooker = null!;
    internal static ConfigEntry<bool> TropiCrawlers = null!;
    internal static ConfigEntry<bool> BeetledRoots = null!;
    internal static ConfigEntry<bool> HelpfulBounces = null!;
    internal static ConfigEntry<bool> AlpineScoutnighter = null!;
    internal static ConfigEntry<bool> CampGhost = null!;
    internal static ConfigEntry<bool> BeetleTweaks = null!;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        ShoreTornadoes = Config.Bind("Hazards", "ShoreTornadoes", true, "Randomly spawn tornadoes in Shore.");
        EveryLooker = Config.Bind("Hazards", "TropicalStalkers", true, "Spawn lookers on Tropics.");
        TropiCrawlers = Config.Bind("Hazards", "TropiCrawlers", true, "Spawns some crawlers on Tropics.");
        BeetledRoots = Config.Bind("Hazard", "BeetledRoots", true, "Spawns extra beetles in the roots");
        HelpfulBounces = Config.Bind("Tweak", "Helpful Bounces", true, "Increases the player boosting momentum");
        AlpineScoutnighter = Config.Bind("Hazard", "Alpine Scoutnighter", true, "Scoutmaster may appear at night in the Alpine, very bonkable. Requires at least 2 players.");
        CampGhost = Config.Bind("Hazard", "Campfire Ghost", true, "Spawns a ghost after every campfire");
        BeetleTweaks = Config.Bind("Hazard", "Beetle Tweaks", true, "Increases the beetles' knockback");

        Harmony harmony = new(Info.Metadata.GUID);

        harmony.PatchAll();
    }
}