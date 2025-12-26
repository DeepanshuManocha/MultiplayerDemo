// QuickMatchLauncher.cs
using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;

public sealed class QuickMatchLauncher : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private Button playButton;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_InputField nameInput;

    [Header("Room")]
    [SerializeField] private byte maxPlayers = 8;
    [SerializeField] private int minPlayersToStart = 2;

    [Header("Matchmaking")]
    [SerializeField] private string roomName = "QuickMatchRoom";
    [SerializeField] private string gameVersion = "v1";

    private const string UserIdKey = "USER_ID";
    private const string PlayerNameKey = "PLAYER_NAME";
    private bool pendingJoin;

    private void Start()
    {
        string userId = EnsureUserId();
        PhotonNetwork.AuthValues = new AuthenticationValues(userId);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        string savedName = PlayerPrefs.GetString(PlayerNameKey, "");
        if (nameInput != null) nameInput.text = savedName;

        SetStatus("Ready");

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
    }

    private string EnsureUserId()
    {
        if (!PlayerPrefs.HasKey(UserIdKey))
        {
            PlayerPrefs.SetString(UserIdKey, Guid.NewGuid().ToString("N"));
            PlayerPrefs.Save();
        }
        return PlayerPrefs.GetString(UserIdKey, "");
    }

    private void OnPlayClicked()
    {
        pendingJoin = true;

        if (playButton != null) playButton.interactable = false;

        ApplyPlayerName();

        // If somehow still in a room (or returning fast), leave first and wait for OnLeftRoom.
        if (PhotonNetwork.InRoom)
        {
            SetStatus("Leaving previous room...");
            PhotonNetwork.LeaveRoom(false);
            return;
        }

        ConnectOrJoin();
    }

    private void ApplyPlayerName()
    {
        string chosen = nameInput != null ? nameInput.text : "";
        chosen = chosen != null ? chosen.Trim() : "";

        if (string.IsNullOrEmpty(chosen))
        {
            int num = UnityEngine.Random.Range(1000, 9999);
            chosen = "Player" + num;
            if (nameInput != null) nameInput.text = chosen;
        }

        PlayerPrefs.SetString(PlayerNameKey, chosen);
        PlayerPrefs.Save();

        PhotonNetwork.NickName = chosen;
    }

    private void ConnectOrJoin()
    {
        SetStatus("Connecting...");

        if (PhotonNetwork.IsConnected)
            JoinOrCreate();
        else
            PhotonNetwork.ConnectUsingSettings();
    }

    private void JoinOrCreate()
    {
        SetStatus("Joining or creating room...");

        RoomOptions opts = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsOpen = true,
            IsVisible = true
        };

        PhotonNetwork.JoinOrCreateRoom(roomName, opts, TypedLobby.Default);
    }

    public override void OnConnectedToMaster()
    {
        if (!pendingJoin) return;
        JoinOrCreate();
    }

    public override void OnJoinedRoom()
    {
        // Set Score=0 on join (if you are using scoring)
        var props = new Hashtable();
        props["Score"] = 0;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        SetStatus($"In room ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})");
        TryStartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetStatus($"In room ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})");
        TryStartGame();
    }

    private void TryStartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.CurrentRoom.PlayerCount >= minPlayersToStart)
            PhotonNetwork.LoadLevel("Game");
        else
            SetStatus("Waiting for 1 more player...");
    }

    public override void OnLeftRoom()
    {
        if (!pendingJoin) return;
        ConnectOrJoin();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        pendingJoin = false;
        ShowError("Join room failed:\n" + message, ResetToReady);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        pendingJoin = false;
        ShowError("Create room failed:\n" + message, ResetToReady);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        pendingJoin = false;
        ShowError("Disconnected:\n" + cause, ResetToReady);
    }

    private void ResetToReady()
    {
        if (playButton != null) playButton.interactable = true;
        SetStatus("Ready");
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    private void ShowError(string msg, Action retry)
    {
        if (SimpleErrorPopup.Instance != null)
            SimpleErrorPopup.Instance.Show(msg, retry);
        else
            Debug.LogError(msg);
    }
}
