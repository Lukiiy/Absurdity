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

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        ShoreTornadoes = Config.Bind("Hazards", "ShoreTornadoes", true, "Randomly spawn tornadoes in Shore.");

        Harmony harmony = new(Info.Metadata.GUID);

        harmony.PatchAll();
    }
}