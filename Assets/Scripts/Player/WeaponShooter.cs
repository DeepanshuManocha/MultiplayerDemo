// WeaponShooter.cs
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public sealed class WeaponShooter : MonoBehaviourPun
{
    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float cooldown = 0.25f;

    [Header("Bullet travel (speed and life define distance)")]
    [SerializeField] private float bulletSpeed = 25f;
    [SerializeField] private float bulletLife = 1.5f;

    [Header("Visuals")]
    [SerializeField] private Vector3 muzzleLocalOffset = new Vector3(0f, 1.2f, 0.7f);

    [Header("Testing")]
    [SerializeField] private bool enableMouseFireInEditor = true;

    private float nextFireTime;
    private PhotonView pv;
    private Camera cam;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (!pv.IsMine) return;

        if (cam == null) cam = Camera.main;

        if (!Application.isMobilePlatform && enableMouseFireInEditor)
        {
            if (Input.GetMouseButton(0))
                TryFire();
        }
    }

    public void TryFire()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + cooldown;

        if (cam == null) return;

        float maxDistance = Mathf.Max(5f, bulletSpeed * bulletLife);

        Vector3 muzzleWorld = transform.TransformPoint(muzzleLocalOffset);

        // Crosshair aim (screen center)
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray aimRay = cam.ScreenPointToRay(screenCenter);

        Vector3 aimPoint = aimRay.origin + aimRay.direction * maxDistance;
        if (Physics.Raycast(aimRay, out RaycastHit camHit, maxDistance))
            aimPoint = camHit.point;

        // Fire from muzzle towards aim point
        Vector3 dir = aimPoint - muzzleWorld;
        if (dir.sqrMagnitude < 0.0001f) dir = cam.transform.forward;
        dir.Normalize();

        Vector3 endPoint = muzzleWorld + dir * maxDistance;

        if (Physics.Raycast(muzzleWorld, dir, out RaycastHit hit, maxDistance))
        {
            endPoint = hit.point;

            PhotonView targetPV = hit.collider.GetComponentInParent<PhotonView>();
            if (targetPV != null && targetPV != pv)
            {
                PlayerHealth targetHealth = targetPV.GetComponent<PlayerHealth>();
                if (targetHealth != null)
                {
                    float travelTime = Vector3.Distance(muzzleWorld, hit.point) / Mathf.Max(0.01f, bulletSpeed);
                    int attackerActor = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1;

                    targetPV.RPC(
                        nameof(PlayerHealth.RPC_TakeDamageDelayed),
                        targetPV.Owner,
                        damage,
                        travelTime,
                        attackerActor
                    );
                }
            }
        }

        pv.RPC(nameof(RPC_SpawnShotVisual), RpcTarget.AllViaServer, muzzleWorld, endPoint);
    }

    [PunRPC]
    private void RPC_SpawnShotVisual(Vector3 from, Vector3 to)
    {
        if (ProjectilePool.Instance == null) return;
        if (!ProjectilePool.Instance.IsReady) return;

        ProjectileVisual p = ProjectilePool.Instance.Get();
        if (p == null) return;

        p.Launch(from, to);
    }
}
