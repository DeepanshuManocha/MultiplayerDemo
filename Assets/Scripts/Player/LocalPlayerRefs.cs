// LocalPlayerRefs.cs (class name kept as LocalPlayerEvents for compatibility)
using System;
using UnityEngine;

public static class LocalPlayerEvents
{
    public static event Action<GameObject> OnLocalPlayerSpawned;

    public static GameObject LocalPlayer { get; private set; }

    public static void SetLocalPlayer(GameObject go)
    {
        if (go == null) return;
        if (ReferenceEquals(LocalPlayer, go)) return;

        LocalPlayer = go;
        OnLocalPlayerSpawned?.Invoke(go);
    }

    public static void Clear(GameObject go)
    {
        if (!ReferenceEquals(LocalPlayer, go)) return;
        LocalPlayer = null;
    }
}
