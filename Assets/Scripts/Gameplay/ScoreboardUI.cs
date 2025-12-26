using System.Text;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;

public sealed class ScoreboardUI : MonoBehaviourPunCallbacks
{

    [SerializeField] private TMP_Text youText;
    [SerializeField] private TMP_Text enemyText;

    private const string ScoreKey = "Score";

    private void Start()
    {
        Refresh();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps != null && changedProps.ContainsKey(ScoreKey))
            Refresh();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) => Refresh();
    public override void OnPlayerLeftRoom(Player otherPlayer) => Refresh();
    public override void OnJoinedRoom() => Refresh();

    private void Refresh()
    {
        if (youText == null || enemyText == null) return;

        Player local = PhotonNetwork.LocalPlayer;
        Player enemy = FindOther(local);

        youText.text = "You: " + GetScore(local);

        if (enemy != null)
            enemyText.text = enemy.NickName + ": " + GetScore(enemy);
        else
            enemyText.text = "Enemy: -";
    }

    private static Player FindOther(Player local)
    {
        Player[] list = PhotonNetwork.PlayerList;
        for (int i = 0; i < list.Length; i++)
        {
            if (local == null || list[i] == null) continue;
            if (list[i].ActorNumber != local.ActorNumber) return list[i];
        }
        return null;
    }

    private static int GetScore(Player p)
    {
        if (p == null || p.CustomProperties == null) return 0;
        if (!p.CustomProperties.ContainsKey(ScoreKey)) return 0;
        return (int)p.CustomProperties[ScoreKey];
    }
}
