using System.Collections;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

[HarmonyPatch(typeof(WindChillZone), "RPCA_ToggleWind")]
internal static class AlpineItemRain
{
    private const float Height = 45f;
    private const float Radius = 16f;

    private static readonly string[] Pool = ["Bugle", "Compass", "Torch", "Rope", "MagicBean", "Item_Coconut_half", "Snowball"];
    private static readonly string[] CoolPool = ["ClimbingSpike", "ChainShooter"];

    private static void Postfix(WindChillZone __instance, bool set, float untilSwitch)
    {
        if (!PhotonNetwork.IsMasterClient || !set || __instance != WindChillZone.instance || !Utils.IsBiome(Biome.BiomeType.Alpine) || Random.value >= .5) return;

        bool anyPlayer = false;

        foreach (Character player in Utils.GetActivePlayers())
        {
            if (!__instance.windZoneBounds.Contains(player.Center)) continue;

            anyPlayer = true;
            break;
        }

        if (!anyPlayer) return; // nobody to apply

        Plugin.Instance?.StartCoroutine(RainItems(untilSwitch));
    }

    private static IEnumerator RainItems(float duration)
    {
        float interval = 2;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!PhotonNetwork.IsMasterClient || !Utils.IsBiome(Biome.BiomeType.Alpine)) yield break;

            foreach (Character player in Utils.GetActivePlayers()) Item(player);

            yield return new WaitForSeconds(interval);

            elapsed += interval;
        }
    }

    private static void Item(Character player)
    {
        Vector3 spawn = default;
        bool found = false;

        for (int attempt = 0; attempt < 5; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle * Radius;
            Vector3 candidate = player.Center + new Vector3(offset.x, Height, offset.y);

            if (Physics.CheckSphere(candidate, .3f, HelperFunctions.GetMask(HelperFunctions.LayerType.Terrain))) continue; // inside solid terrain, try another spot

            spawn = candidate;
            found = true;

            break;
        }

        if (!found) return;

        GameObject? item = Utils.TrySpawnObject(Pool[Random.Range(0, Pool.Length)], spawn, Random.rotation);
        if (item == null) return;

        PhotonView view = item.GetComponent<PhotonView>();
        if (view == null) return;

        view.RPC("SetKinematicRPC", RpcTarget.AllBuffered, false, spawn, item.transform.rotation);

        if (item.GetComponent<Bonkable>() == null) {
            Bonkable bonkable = item.AddComponent<Bonkable>();

            bonkable.bonk = [];
            bonkable.bonkForce = 150f;
            bonkable.ragdollTime = .4f;
            bonkable.minBonkVelocity = 6f;
            bonkable.bonkRange = 2f;
        }

        item.AddComponent<AutoDespawn>().Init(player);
    }

    private sealed class AutoDespawn : MonoBehaviour
    {
        private Character player = null!;

        public void Init(Character player) => this.player = player;

        public void FixedUpdate()
        {
            if (player == null || transform.position.y < player.Center.y - 3f)
            {
                enabled = false; // lol

                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}