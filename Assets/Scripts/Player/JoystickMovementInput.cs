// JoystickMovementInput.cs
using UnityEngine;

public sealed class JoystickMovementInput : MonoBehaviour
{
    [SerializeField] private Joystick joystick;

    private PlayerMotor motor;

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
        if (joystick == null) joystick = GetComponent<Joystick>();

        if (LocalPlayerEvents.LocalPlayer != null)
            HandleLocalPlayerSpawned(LocalPlayerEvents.LocalPlayer);
    }

    private void HandleLocalPlayerSpawned(GameObject playerGo)
    {
        if (playerGo == null) return;
        motor = playerGo.GetComponent<PlayerMotor>();
    }

    private void Update()
    {
        if (motor == null || joystick == null) return;

        Vector2 move = new Vector2(joystick.Horizontal, joystick.Vertical);
        motor.SetMoveInput(move);
    }
}
