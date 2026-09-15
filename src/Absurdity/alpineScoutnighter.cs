using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

internal sealed class AlpineScoutnighter : MonoBehaviour
{
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
    private static class Apply
    {
        private static void Postfix(RunManager __instance)
        {
            if (__instance == null || !Plugin.AlpineScoutnighter.Value) return;

            if (__instance.GetComponent<AlpineScoutnighter>() == null) __instance.gameObject.AddComponent<AlpineScoutnighter>();
        }
    }

    private bool wasNight;

    private void Update()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient || MapHandler.Instance == null || DayNightManager.instance == null) return; // inRoom is intended, as I don't want to handle this in solo runs.

        if (MapHandler.Instance.GetCurrentBiome() != Biome.BiomeType.Alpine)
        {
            wasNight = false;
            return;
        }

        bool isNight = DayNightManager.instance.isDay < .5f;
        if (isNight && !wasNight)
        {
            List<Character> players = Utils.GetActivePlayers();
            if (players == null || players.Count == 0) return;

            Character target = players[UnityEngine.Random.Range(0, players.Count)];
            Vector2 offset = UnityEngine.Random.insideUnitCircle * 20f;
            Vector3 spawn = target.Center + new Vector3(offset.x, 4f, offset.y);

            GameObject scoutmasterObj = PhotonNetwork.InstantiateRoomObject("Character_Scoutmaster", spawn, Quaternion.identity, 0);
            if (scoutmasterObj == null) return;

            Scoutmaster scoutmaster = scoutmasterObj.GetComponent<Scoutmaster>();
            Character scoutmasterChar = scoutmasterObj.GetComponent<Character>();
            if (scoutmaster == null || scoutmasterChar == null) return;

            scoutmasterChar.data.spawnPoint = target.transform;
            scoutmasterObj.GetComponent<PhotonView>()?.RPC("RPCA_SetCurrentTarget", RpcTarget.All, target.photonView.ViewID, 0f);

            Plugin.Log.LogDebug($"Spawned scoutmaster near {target.name} targeting {target.name}");
        }

        wasNight = isNight;
    }
}