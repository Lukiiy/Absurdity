using Photon.Pun;

namespace Absurdity;

public class EventHooks
{
    public EventHooks()
    {
        GlobalEvents.OnCharacterDied += OnPlayerDied;
    }

    public static void OnPlayerDied(Character character)
    {
        if (!PhotonNetwork.IsMasterClient || !character.data.isSkeleton) return;

        Utils.TrySpawnObject("Egg", character.Center, UnityEngine.Random.rotation);
    }
}