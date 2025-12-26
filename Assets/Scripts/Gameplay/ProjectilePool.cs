// ProjectilePool.cs
using System.Collections.Generic;
using UnityEngine;

public sealed class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    public bool IsReady { get; private set; }

    private ProjectileVisual prefab;
    private readonly Queue<ProjectileVisual> pool = new Queue<ProjectileVisual>(64);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsReady = false;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Init(ProjectileVisual projectilePrefab, int prewarmCount)
    {
        prefab = projectilePrefab;
        pool.Clear();

        int count = Mathf.Max(0, prewarmCount);
        for (int i = 0; i < count; i++)
            CreateOne();

        IsReady = true;
    }

    private ProjectileVisual CreateOne()
    {
        ProjectileVisual p = Instantiate(prefab, transform);
        p.gameObject.SetActive(false);
        p.SetPool(this);
        pool.Enqueue(p);
        return p;
    }

    public ProjectileVisual Get()
    {
        if (!IsReady || prefab == null) return null;

        if (pool.Count == 0)
            CreateOne();

        ProjectileVisual p = pool.Dequeue();
        p.gameObject.SetActive(true);
        return p;
    }

    public void Return(ProjectileVisual p)
    {
        if (p == null) return;
        p.gameObject.SetActive(false);
        pool.Enqueue(p);
    }
}
