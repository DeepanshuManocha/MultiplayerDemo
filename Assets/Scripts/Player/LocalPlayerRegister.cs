using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public sealed class LocalPlayerRegister : MonoBehaviour
{
    private PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (pv == null || !pv.IsMine) return;
        LocalPlayerEvents.SetLocalPlayer(gameObject);
    }

    private void OnDestroy()
    {
        if (pv != null && pv.IsMine)
            LocalPlayerEvents.Clear(gameObject);
    }
}