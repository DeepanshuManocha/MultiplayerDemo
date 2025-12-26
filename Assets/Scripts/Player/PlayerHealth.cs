// PlayerHealth.cs
using System.Collections;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public sealed class PlayerHealth : MonoBehaviourPun, IPunObservable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 2f;

    public int CurrentHealth { get; private set; }
    public bool IsDeadVisual { get; private set; }

    private bool deadLocal;
    private int lifeToken;
    private int lastAttackerActorNumber;

    private PlayerMotor cachedMotor;
    private WeaponShooter cachedShooter;
    private PlayerNetTransform cachedNet;
    private Renderer[] cachedRenderers;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        IsDeadVisual = false;
        deadLocal = false;
        lifeToken = 0;
        lastAttackerActorNumber = -1;

        cachedMotor = GetComponent<PlayerMotor>();
        cachedShooter = GetComponent<WeaponShooter>();
        cachedNet = GetComponent<PlayerNetTransform>();
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Unregister(this);
    }

    [PunRPC]
    public void RPC_TakeDamage(int dmg)
    {
        if (!photonView.IsMine) return;
        if (deadLocal) return;

        dmg = Mathf.Abs(dmg);
        ApplyDamageLocal(dmg);
    }

    [PunRPC]
    public void RPC_TakeDamageDelayed(int dmg, float delay, int attackerActorNumber)
    {
        if (!photonView.IsMine) return;

        dmg = Mathf.Abs(dmg);
        if (delay < 0f) delay = 0f;

        lastAttackerActorNumber = attackerActorNumber;

        int token = lifeToken;
        StartCoroutine(DelayDamageRoutine(dmg, delay, token));
    }

    private IEnumerator DelayDamageRoutine(int dmg, float delay, int token)
    {
        yield return new WaitForSeconds(delay);

        if (token != lifeToken) yield break;
        if (deadLocal) yield break;

        ApplyDamageLocal(dmg);
    }

    private void ApplyDamageLocal(int dmg)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - dmg);

        if (CurrentHealth <= 0)
            StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        if (deadLocal) yield break;

        deadLocal = true;
        lifeToken++;

        int victimActor = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1;
        if (lastAttackerActorNumber > 0 && lastAttackerActorNumber != victimActor)
        {
            ScoreManager.ReportKillToMaster(lastAttackerActorNumber);
        }
        lastAttackerActorNumber = -1;

        photonView.RPC(nameof(RPC_SetDeadVisual), RpcTarget.AllViaServer, true);

        yield return new WaitForSeconds(respawnDelay);

        Transform sp = GameManager.Instance != null ? GameManager.Instance.GetBestRespawn(this) : null;
        Vector3 spawnPos = sp != null ? sp.position : Vector3.zero;
        Quaternion spawnRot = sp != null ? sp.rotation : Quaternion.identity;

        CurrentHealth = maxHealth;

        photonView.RPC(nameof(RPC_RespawnAll), RpcTarget.AllViaServer, spawnPos, spawnRot, CurrentHealth);

        deadLocal = false;
    }

    [PunRPC]
    private void RPC_SetDeadVisual(bool isDead)
    {
        IsDeadVisual = isDead;
        SetVisible(!isDead);

        if (!photonView.IsMine) return;

        if (cachedMotor != null)
        {
            if (isDead) cachedMotor.SetMoveInput(Vector2.zero);
            cachedMotor.enabled = !isDead;
        }

        if (cachedShooter != null) cachedShooter.enabled = !isDead;
    }

    [PunRPC]
    private void RPC_RespawnAll(Vector3 pos, Quaternion rot, int newHealth)
    {
        CurrentHealth = newHealth;
        IsDeadVisual = false;

        if (cachedNet != null) cachedNet.SnapTo(pos, rot);
        else transform.SetPositionAndRotation(pos, rot);

        SetVisible(true);

        if (!photonView.IsMine) return;

        if (cachedMotor != null) cachedMotor.enabled = true;
        if (cachedShooter != null) cachedShooter.enabled = true;
    }

    private void SetVisible(bool visible)
    {
        if (cachedRenderers == null) return;

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] != null)
                cachedRenderers[i].enabled = visible;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(CurrentHealth);
        else CurrentHealth = (int)stream.ReceiveNext();
    }
}
