// OrbitFollowCamera.cs
using UnityEngine;

public sealed class OrbitFollowCamera : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -4.5f);
    [SerializeField] private float followSmooth = 18f;

    [Header("Look")]
    [SerializeField] private bool requireHoldToLook = true;
    [SerializeField] private int mouseButton = 0;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float touchSensitivity = 0.12f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Touch zones")]
    [SerializeField] private bool joystickBottomRight = true;

    [Header("Debug")]
    [SerializeField] private bool logWhenLocked = false;

    private Transform target;
    private float yaw;
    private float pitch = 20f;

    private void OnEnable()
    {
        LocalPlayerEvents.OnLocalPlayerSpawned += HandleLocalPlayerSpawned;
    }

    private void OnDisable()
    {
        LocalPlayerEvents.OnLocalPlayerSpawned -= HandleLocalPlayerSpawned;
    }

    private void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (LocalPlayerEvents.LocalPlayer != null)
            HandleLocalPlayerSpawned(LocalPlayerEvents.LocalPlayer);
    }

    private void HandleLocalPlayerSpawned(GameObject playerGo)
    {
        if (playerGo == null) return;

        Transform camTarget = playerGo.transform.Find("CameraTarget");
        target = camTarget != null ? camTarget : playerGo.transform;

        if (logWhenLocked && target != null)
            Debug.Log("[Camera] Locked to: " + target.name);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleLook();

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPos = target.position + rot * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSmooth);
        transform.LookAt(target.position);
    }

    private void HandleLook()
    {
        bool canMouseLook = !requireHoldToLook || Input.GetMouseButton(mouseButton);

        if (canMouseLook)
        {
            float dx = Input.GetAxisRaw("Mouse X");
            float dy = Input.GetAxisRaw("Mouse Y");

            yaw += dx * mouseSensitivity;
            pitch -= dy * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        if (Input.touchCount <= 0) return;

        Touch t = Input.GetTouch(0);

        if (t.position.x > Screen.width * 0.5f) return;

        if (t.phase == TouchPhase.Moved)
        {
            Vector2 d = t.deltaPosition;
            yaw += d.x * touchSensitivity;
            pitch -= d.y * touchSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }
}
