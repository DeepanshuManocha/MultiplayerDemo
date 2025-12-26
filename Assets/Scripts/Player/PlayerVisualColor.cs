using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public sealed class PlayerVisualColor : MonoBehaviourPun
{
    [Header("Renderers")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Renderer headRenderer;

    [Header("Local Player Colors")]
    [SerializeField] private Color localBodyColor = new Color(0.2f, 0.8f, 1f, 1f);
    [SerializeField] private Color localHeadColor = new Color(0.15f, 0.95f, 0.6f, 1f);

    [Header("Enemy Colors")]
    [SerializeField] private Color enemyBodyColor = new Color(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color enemyHeadColor = new Color(1f, 0.75f, 0.2f, 1f);

    private Material[] bodyInstanced;
    private Material[] headInstanced;

    private void Start()
    {
        ResolveRenderersOnce();
        ApplyColors();
    }

    private void ResolveRenderersOnce()
    {
        if (bodyRenderer == null)
        {
            Transform t = transform.Find("Body");
            if (t != null) bodyRenderer = t.GetComponent<Renderer>();
        }

        if (headRenderer == null)
        {
            Transform t = transform.Find("Head");
            if (t != null) headRenderer = t.GetComponent<Renderer>();
        }
    }

    private void ApplyColors()
    {
        bool isLocal = photonView.IsMine;

        Color body = isLocal ? localBodyColor : enemyBodyColor;
        Color head = isLocal ? localHeadColor : enemyHeadColor;

        InstanceAndTint(bodyRenderer, ref bodyInstanced, body);
        InstanceAndTint(headRenderer, ref headInstanced, head);
    }

    private void InstanceAndTint(Renderer r, ref Material[] cache, Color c)
    {
        if (r == null) return;

        Material[] shared = r.sharedMaterials;
        if (shared == null || shared.Length == 0) return;

        cache = new Material[shared.Length];

        for (int i = 0; i < shared.Length; i++)
        {
            Material src = shared[i];
            if (src == null) continue;

            Material inst = new Material(src);
            SetColor(inst, c);
            cache[i] = inst;
        }

        r.materials = cache;
    }

    private void SetColor(Material m, Color c)
    {
        if (m == null) return;

        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
    }

    private void OnDestroy()
    {
        DestroyMats(bodyInstanced);
        DestroyMats(headInstanced);
    }

    private void DestroyMats(Material[] mats)
    {
        if (mats == null) return;

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] != null)
                Destroy(mats[i]);
        }
    }
}
