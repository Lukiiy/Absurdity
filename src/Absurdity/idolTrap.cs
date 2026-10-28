using System.Collections;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

[HarmonyPatch(typeof(Item), "RequestPickup")]
internal static class IdolTrap
{
    private static bool triggered;

    private static void Postfix(Item __instance, PhotonView characterView)
    {
        if (!PhotonNetwork.IsMasterClient || triggered) return;
        if (__instance == null || !__instance.name.Contains("AncientIdol")) return;

        Character character = characterView.GetComponent<Character>();
        if (character == null) return;

        triggered = true;

        Plugin.Instance?.StartCoroutine(SpamPetrified(character));
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
    private static class RunReset
    {
        private static void Postfix() => triggered = false;
    }

    private static IEnumerator SpamPetrified(Character character)
    {
        for (int i = 0; i < 3; i++)
        {
            if (character == null) yield break;

            Utils.SpawnPetrified(character, character.refs.ragdoll.bodySpawnPoint.position + Vector3.up * 8f, character.refs.ragdoll.bodySpawnPoint.rotation, Random.value < .5);

            PhotonNetwork.Instantiate("MushroomZombie", character.refs.ragdoll.bodySpawnPoint.position, character.refs.ragdoll.bodySpawnPoint.rotation, 0).GetComponent<MushroomZombie>();

            yield return new WaitForSeconds(.2f);
        }
    }
}