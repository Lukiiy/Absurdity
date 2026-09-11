using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace Absurdity;

[HarmonyPatch(typeof(Spawner), nameof(Spawner.SpawnItems))]
public static class ExtraLuggageItems
{
    private static readonly string[] BonusItems = ["Antidote", "Balloon", "Dynamite", "CloudFungus", "WarpFungus", "Parachute", "Rocketpack", "RopeShooter"]; // dis: "Clusterberry Black"
    private static readonly string[] AlpineBonus = ["MagicBean", "Flag_Plantable_Checkpoint", "PortableStovetopItem"];
    private static readonly string[] MesaBonus = ["Glider", "BounceShroom", "RopeSpool"];

    private static readonly System.Random Random = new();

    [HarmonyPostfix]
    private static void Postfix(Spawner __instance, List<PhotonView> __result)
    {
        if (__instance is not Luggage || __result == null || __result.Count != 1 || !PhotonNetwork.IsMasterClient || Random.NextDouble() >= .4) return; // only 1 item luggages + 40% chance

        Transform spawn = __result[0].transform;
        string[] pool = BonusItems;
        Biome.BiomeType biome = Singleton<MapHandler>.Instance.GetCurrentBiome();

        if (biome == Biome.BiomeType.Alpine) pool = BonusItems.Concat(AlpineBonus).ToArray(); else if (biome == Biome.BiomeType.Mesa) pool = BonusItems.Concat(MesaBonus).ToArray();

        for (int i = 0; i < 2; i++)
        {
            Vector3 position = spawn.position + Vector3.up * (.25f + i * .15f);

            GameObject? item = Utils.TrySpawnObject(pool[Random.Next(pool.Length)], position, UnityEngine.Random.rotation);
            if (item == null) continue;

            PhotonView view = item.GetComponent<PhotonView>();
            if (view == null) continue;

            view.RPC("SetKinematicRPC", RpcTarget.AllBuffered, false, position, item.transform.rotation);

            Vector3 force = Vector3.up * UnityEngine.Random.Range(2.5f, 4f) + UnityEngine.Random.insideUnitSphere * .5f;

            force.y = Mathf.Abs(force.y);
            view.RPC("ApplyForceRPC", RpcTarget.AllBuffered, force, UnityEngine.Random.insideUnitSphere * .25f);
        }
    }
}