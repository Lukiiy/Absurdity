using Photon.Pun;

namespace Absurdity;

public class EventHooks
{
    public EventHooks()
    {
        GlobalEvents.OnCharacterDied += OnPlayerDied;
        GlobalEvents.OnItemThrown += OnItemThrown;
        GlobalEvents.OnBugleTooted += OnBugleTooted;
    }

    public static void OnPlayerDied(Character character)
    {
        if (!PhotonNetwork.IsMasterClient || !character.data.isSkeleton) return;

        Utils.TrySpawnObject("Egg", character.Center, UnityEngine.Random.rotation);
    }

    private static void OnItemThrown(Item item)
    {
        if (item == null || item.GetComponent<Bonkable>() != null) return;

        Bonkable bonkable = item.gameObject.AddComponent<Bonkable>();

        bonkable.minBonkVelocity = 5f;
        bonkable.decreaseMinVelocityIfRecentlyThrown = false;
        bonkable.ragdollTime = 1f;
        bonkable.bonkForce = 100f;
        bonkable.bonkRange = 1f;
    }

    private static void OnBugleTooted(Item bugle)
    {
        if (!PhotonNetwork.IsMasterClient || bugle == null || bugle.GetComponent<MagicBugle>() != null) return;

        bugle.holderCharacter?.Fall(2f);
    }
}