// PlayerHUD.cs
using TMPro;
using UnityEngine;
using Photon.Pun;

public sealed class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text healthText;

    private PlayerHealth localHealth;
    private int lastHp = int.MinValue;
    private string playerName = "";

    private void OnEnable()
    {
        LocalPlayerEvents.OnLocalPlayerSpawned += HandleLocalPlayerSpawned;
    }

    private void OnDisable()
    {
        LocalPlayerEvents.OnLocalPlayerSpawned -= HandleLocalPlayerSpawned;
    }

    private void Start()
    {
        if (LocalPlayerEvents.LocalPlayer != null)
            HandleLocalPlayerSpawned(LocalPlayerEvents.LocalPlayer);

        RefreshName(force: true);
        RefreshHealth(force: true);
    }

    private void HandleLocalPlayerSpawned(GameObject playerGo)
    {
        if (playerGo == null) return;

        localHealth = playerGo.GetComponent<PlayerHealth>();

        RefreshName(force: true);
        RefreshHealth(force: true);
    }

    private void Update()
    {
        RefreshName(force: false);
        RefreshHealth(force: false);
    }

    private void RefreshName(bool force)
    {
        string n = PhotonNetwork.NickName ?? "";
        if (!force && n == playerName) return;

        playerName = n;
        if (nameText != null) nameText.text = "Name: " + playerName;
    }

    private void RefreshHealth(bool force)
    {
        int hp = localHealth != null ? localHealth.CurrentHealth : 0;
        if (!force && hp == lastHp) return;

        lastHp = hp;
        if (healthText != null) healthText.text = "Health: " + hp;
    }
}
