// MobileFireButton.cs
using UnityEngine;
using UnityEngine.UI;

public sealed class FireButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private WeaponShooter shooter;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(OnPressed);
    }

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
        if (LocalPlayerEvents.LocalPlayer != null)
            HandleLocalPlayerSpawned(LocalPlayerEvents.LocalPlayer);
    }

    private void HandleLocalPlayerSpawned(GameObject playerGo)
    {
        if (playerGo == null) return;
        shooter = playerGo.GetComponent<WeaponShooter>();
    }

    private void OnPressed()
    {
        if (shooter == null) return;
        shooter.TryFire();
    }
}
