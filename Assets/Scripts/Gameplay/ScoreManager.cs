using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public sealed class ScoreManager : MonoBehaviour, IOnEventCallback
{
    private const byte KillEventCode = 1;
    private const byte GameEndEventCode = 2;
    private const string ScoreKey = "Score";

    [SerializeField] private int maxScore = 5;
    [SerializeField] private GameOverPopup gameOverPopup;

    private bool gameEnded;

    private void Awake()
    {
        gameEnded = false;
    }

    private void Start()
    {
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public static void ReportKillToMaster(int killerActorNumber)
    {
        if (killerActorNumber <= 0) return;

        RaiseEventOptions opts = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(KillEventCode, killerActorNumber, opts, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent == null) return;

        if (photonEvent.Code == KillEventCode)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (gameEnded) return;

            int killerActor = (int)photonEvent.CustomData;

            Player killer = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.GetPlayer(killerActor) : null;
            if (killer == null) return;

            int current = GetScore(killer);
            int next = current + 1;

            Hashtable props = new Hashtable();
            props[ScoreKey] = next;
            killer.SetCustomProperties(props);

            if (next >= maxScore)
            {
                gameEnded = true;

                RaiseEventOptions endOpts = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent(GameEndEventCode, killerActor, endOpts, SendOptions.SendReliable);
            }

            return;
        }

        if (photonEvent.Code == GameEndEventCode)
        {
            int winnerActorNumber = (int)photonEvent.CustomData;
            ShowGameOver(winnerActorNumber);
        }
    }

    private void ShowGameOver(int winnerActorNumber)
    {
        if (gameOverPopup == null) return;

        Player local = PhotonNetwork.LocalPlayer;
        bool isWinner = local != null && local.ActorNumber == winnerActorNumber;

        if (isWinner) gameOverPopup.ShowWin();
        else gameOverPopup.ShowLose();
    }

    private static int GetScore(Player p)
    {
        if (p == null || p.CustomProperties == null) return 0;
        if (!p.CustomProperties.ContainsKey(ScoreKey)) return 0;
        return (int)p.CustomProperties[ScoreKey];
    }
}
