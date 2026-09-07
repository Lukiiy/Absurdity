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

    private const float bonkRadius = 1.75f;
    private bool wasNight;

    private void OnEnable() => GlobalEvents.OnItemThrown += OnItemThrown;
    private void OnDisable() => GlobalEvents.OnItemThrown -= OnItemThrown;

    private void Update()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return; // This is intended, as I don't want to handle this in solo runs.
        if (MapHandler.Instance == null || DayNightManager.instance == null) return;

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

    private static void OnItemThrown(Item item)
    {
        if (item == null || item.rig == null || !PhotonNetwork.InRoom) return;

        ThrownScoutmasterBonk tracker = item.GetComponent<ThrownScoutmasterBonk>() ?? item.gameObject.AddComponent<ThrownScoutmasterBonk>();

        tracker.Begin(item);
    }

    private sealed class ThrownScoutmasterBonk : MonoBehaviour
    {
        private static readonly Collider[] HitBuffer = new Collider[16];

        private Item? _item;
        private float _startedAt;
        private float _lastBonkAt = float.NegativeInfinity;

        internal void Begin(Item item)
        {
            _item = item;
            _startedAt = Time.time;
        }

        private void FixedUpdate()
        {
            if (_item == null || _item.rig == null || _item.itemState != ItemState.Ground)
            {
                Destroy(this);
                return;
            }

            if (Time.time - _startedAt > 8f)
            {
                Destroy(this);
                return;
            }

            if (_item.rig.linearVelocity.sqrMagnitude < 4f || Time.time - _lastBonkAt < .35f) return;

            int count = Physics.OverlapSphereNonAlloc(_item.Center(), bonkRadius, HitBuffer, ~0, QueryTriggerInteraction.Ignore); // this one's for the nerds

            for (int i = 0; i < count; i++)
            {
                Character character = HitBuffer[i].GetComponentInParent<Character>();
                if (character == null || !character.isScoutmaster || character.data.dead) continue;

                _lastBonkAt = Time.time;

                Bonk(character);
                Destroy(this);

                return;
            }
        }

        private static void Bonk(Character scoutmaster)
        {
            PhotonView view = scoutmaster.photonView;
            if (view == null) return;

            scoutmaster.Fall(3f);

            List<Character> players = Utils.GetActivePlayers();
            if (players == null || players.Count == 0) return;

            int targetIdx = UnityEngine.Random.Range(0, players.Count);
            Character target = players[targetIdx];

            if (target == scoutmaster)
            {
                if (players.Count == 1) return;

                target = players[(targetIdx + 1) % players.Count];
            }

            view.RPC("RPCA_SetCurrentTarget", RpcTarget.All, target.photonView.ViewID, 0f);
        }
    }
}