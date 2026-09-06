using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace Absurdity;

internal static class Utils
{
    internal static bool InRoomAndReady() => PhotonNetwork.InRoom && MapHandler.ExistsAndInitialized;

    internal static bool IsBiome(Biome.BiomeType biome) => InRoomAndReady() && Singleton<MapHandler>.Instance.GetCurrentBiome() == biome;

    internal static List<Character> GetActivePlayers()
    {
        List<Character> players = [];

        foreach (Character character in PlayerHandler.GetAllPlayerCharacters())
        {
            if (character == null || character.data.dead || !character.data.fullyConscious) continue;

            players.Add(character);
        }

        return players;
    }

    internal static bool IsOnMountain(Vector3 position)
    {
        MapHandler.MapSegment segment = MapHandler.CurrentMapSegment;
        if (segment == null) return false;

        Transform spawn = MapHandler.CurrentBaseCampSpawnPoint;
        if (spawn == null) return false;

        Vector3 start = spawn.transform.position;

        return Vector2.Distance(new(position.x, position.z), new(start.x, start.z)) > 210f;
    }

    internal static Vector3 GetPlayerCenter(List<Character> players)
    {
        if (players.Count == 0) return Character.localCharacter != null ? Character.localCharacter.Center : Vector3.zero;

        Vector3 sum = Vector3.zero;
        for (int i = 0; i < players.Count; i++) sum += players[i].Center;

        return sum / players.Count;
    }

    internal static bool TryGetGround(Vector3 probe, out Vector3 ground) // PLEASE JUST PLEASE WORK
    {
        RaycastHit hit = HelperFunctions.GetGroundPosRaycast(probe + Vector3.up * 100f, HelperFunctions.LayerType.TerrainMap);

        if (hit.transform != null)
        {
            ground = hit.point + hit.normal * .12f;

            return true;
        }

        ground = probe;
        return false;
    }

    internal static GameObject? TrySpawnObject(string prefabName, Vector3 position, Quaternion rotation)
    {
        Exception? lastException = null;

        try
        {
            return PhotonNetwork.InstantiateItemRoom(prefabName, position, rotation);
        }
        catch (Exception ex)
        {
            lastException = ex;
        }

        try
        {
            return PhotonNetwork.InstantiateRoomObject(prefabName, position, rotation, 0);
        }
        catch (Exception ex)
        {
            lastException = ex;
        }

        try
        {
            return PhotonNetwork.Instantiate(prefabName, position, rotation, 0);
        }
        catch (Exception ex)
        {
            lastException = ex;
        }

        Plugin.Log.LogWarning($"Could not network-spawn '{prefabName}' at {position}: {lastException?.Message}");
        return null;
    }
}