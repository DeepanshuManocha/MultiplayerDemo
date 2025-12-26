// RemoteAssetBundleLoader.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public sealed class RemoteAssetBundleLoader : MonoBehaviour
{
    [Header("Remote")]
    [SerializeField] private string bundleUrl;
    [SerializeField] private string prefabNameInBundle = "Ground";

    [Header("Spawn")]
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private Quaternion spawnRotation = default;

    [Header("Scene refs")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject loadedMarker;

    private AssetBundle loadedBundle;
    private GameObject spawned;
    private bool isLoading;

    private void Awake()
    {
        if (loadedMarker != null) loadedMarker.SetActive(false);
    }

    private void Start()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        StartLoad();
    }

    private void StartLoad()
    {
        if (isLoading) return;
        isLoading = true;
        StartCoroutine(DownloadAndSpawn());
    }

    private IEnumerator DownloadAndSpawn()
    {
        if (string.IsNullOrEmpty(bundleUrl))
        {
            isLoading = false;
            ShowError("AssetBundle URL is empty.", Retry);
            yield break;
        }

        using (UnityWebRequest req = UnityWebRequest.Get(bundleUrl))
        {
            req.timeout = 20;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                isLoading = false;
                ShowError("Bundle download failed:\n" + req.error, Retry);
                yield break;
            }

            byte[] bytes = req.downloadHandler.data;
            if (bytes == null || bytes.Length == 0)
            {
                isLoading = false;
                ShowError("Bundle download returned empty data.", Retry);
                yield break;
            }

            var bundleCreate = AssetBundle.LoadFromMemoryAsync(bytes);
            yield return bundleCreate;

            loadedBundle = bundleCreate.assetBundle;
            if (loadedBundle == null)
            {
                isLoading = false;
                ShowError("AssetBundle.LoadFromMemory failed (wrong file or platform mismatch).", Retry);
                yield break;
            }

            GameObject prefab = null;
            GameObject[] allGos = loadedBundle.LoadAllAssets<GameObject>();

            for (int i = 0; i < allGos.Length; i++)
            {
                if (allGos[i] != null && allGos[i].name == prefabNameInBundle)
                {
                    prefab = allGos[i];
                    break;
                }
            }

            if (prefab == null && allGos.Length > 0)
                prefab = allGos[0];

            if (prefab == null)
            {
                isLoading = false;
                ShowError("No GameObject prefab found in bundle.", Retry);
                yield break;
            }

            if (spawned != null) Destroy(spawned);
            spawned = Instantiate(prefab, spawnPosition, spawnRotation);

            if (loadedMarker != null) loadedMarker.SetActive(true);

            if (gameManager != null)
                gameManager.SpawnLocalPlayer();

            isLoading = false;
        }
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
