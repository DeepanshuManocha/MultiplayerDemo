// ProjectileVisual.cs
using UnityEngine;

public sealed class ProjectileVisual : MonoBehaviour
{
    [SerializeField] private float speed = 25f;

    private ProjectilePool pool;
    private Vector3 dir;
    private float totalDist;
    private float traveled;
    private float life;
    private float lifeLimit;

    public void SetPool(ProjectilePool p)
    {
        pool = p;
    }

    public void Launch(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        totalDist = delta.magnitude;

        dir = totalDist > 0.0001f ? (delta / totalDist) : Vector3.forward;

        transform.position = from;
        transform.forward = dir;

        traveled = 0f;
        life = 0f;

        float t = totalDist / Mathf.Max(0.01f, speed);
        lifeLimit = t + 0.05f;
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;

        traveled += step;
        life += Time.deltaTime;

        transform.position += dir * step;

        if (traveled >= totalDist || life >= lifeLimit)
        {
            if (pool != null) pool.Return(this);
            else gameObject.SetActive(false);
        }
    }
}
