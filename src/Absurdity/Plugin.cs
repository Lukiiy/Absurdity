namespace Absurdity;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static Plugin Instance { get; private set; } = null!;
    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        Harmony harmony = new(Info.Metadata.GUID);

        harmony.PatchAll();
    }
}