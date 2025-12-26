// AddressablesProjectileLoader.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class AddressablesProjectileLoader : MonoBehaviour
{
    [Header("Addressables")]
    [SerializeField] private string projectileKey = "ProjectileVisual";
    [SerializeField] private int prewarmCount = 30;

    [Header("Scene refs")]
    [SerializeField] private ProjectilePool pool;
    [SerializeField] private GameObject loadedMarker;

    private bool isLoading;

    private void Awake()
    {
        if (loadedMarker != null && loadedMarker != gameObject)
            loadedMarker.SetActive(false);
    }

    private void Start()
    {
        if (pool == null) pool = ProjectilePool.Instance;

        if (pool == null)
        {
            ShowError("ProjectilePool missing in scene.", Retry);
            return;
        }

        StartLoad();
    }

    private void StartLoad()
    {
        if (isLoading) return;
        isLoading = true;
        StartCoroutine(LoadAndInit());
    }

    private IEnumerator LoadAndInit()
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(projectileKey);
        yield return handle;

        isLoading = false;

        if (!handle.IsValid() || handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
        {
            ShowError("Addressables load failed for:\n" + projectileKey, Retry);
            yield break;
        }

        ProjectileVisual pv = handle.Result.GetComponent<ProjectileVisual>();
        if (pv == null)
        {
            ShowError("Loaded addressable missing ProjectileVisual.", Retry);
            yield break;
        }

        pool.Init(pv, prewarmCount);

        if (loadedMarker != null && loadedMarker != gameObject)
            loadedMarker.SetActive(true);
    }

    private void Retry()
    {
        StartLoad();
    }

    private void ShowError(string msg, Action retry)
    {
        if (SimpleErrorPopup.Instance != null)
            SimpleErrorPopup.Instance.Show(msg, retry);
        else
            Debug.LogError(msg);
    }
}
