// GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Spawning")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnAvoidRadius = 3f;

    private readonly List<PlayerHealth> players = new List<PlayerHealth>(8);
    private bool spawnedLocal;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SpawnLocalPlayer()
    {
        if (spawnedLocal) return;
        spawnedLocal = true;

        Transform sp = GetInitialSpawn(PhotonNetwork.LocalPlayer.ActorNumber);

        Vector3 pos = sp != null ? sp.position : Vector3.zero;
        Quaternion rot = sp != null ? sp.rotation : Quaternion.identity;

        GameObject go = PhotonNetwork.Instantiate("Player", pos, rot);
        LocalPlayerEvents.SetLocalPlayer(go);
    }

    private Transform GetInitialSpawn(int actorNumber)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        int index = (actorNumber - 1) % spawnPoints.Length;
        return spawnPoints[index];
    }

    public void Register(PlayerHealth ph)
    {
        if (ph == null) return;
        if (!players.Contains(ph)) players.Add(ph);
    }

    public void Unregister(PlayerHealth ph)
    {
        if (ph == null) return;
        players.Remove(ph);
    }

    public Transform GetBestRespawn(PlayerHealth requester)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;

        int bestIndex = 0;
        float bestScore = -1f;

        for (int s = 0; s < spawnPoints.Length; s++)
        {
            Vector3 spPos = spawnPoints[s].position;

            float nearestAliveDist = float.MaxValue;
            bool tooClose = false;

            for (int i = 0; i < players.Count; i++)
            {
                PlayerHealth other = players[i];
                if (other == null) continue;
                if (other == requester) continue;
                if (other.IsDeadVisual) continue;

                float d = Vector3.Distance(spPos, other.transform.position);
                if (d < nearestAliveDist) nearestAliveDist = d;
                if (d < spawnAvoidRadius) tooClose = true;
            }

            if (nearestAliveDist == float.MaxValue) nearestAliveDist = 9999f;

            float score = nearestAliveDist;
            if (tooClose) score *= 0.25f;

            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = s;
            }
        }

        return spawnPoints[bestIndex];
    }
}
