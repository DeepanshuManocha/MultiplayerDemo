// PlayerMotor.cs
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public sealed class PlayerMotor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private bool faceCameraYaw = true;
    [SerializeField] private float turnSpeed = 18f;

    [Header("Gravity (basic)")]
    [SerializeField] private float gravity = -18f;

    private CharacterController cc;
    private PhotonView pv;

    private float verticalVel;
    private Vector2 moveInput;
    private Transform cam;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        pv = GetComponent<PhotonView>();
        verticalVel = 0f;
        moveInput = Vector2.zero;
    }

    private void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = Vector2.ClampMagnitude(input, 1f);
    }

    private void Update()
    {
        if (!pv.IsMine) return;
        if (cc == null || !cc.enabled) return;

        if (cam == null && Camera.main != null) cam = Camera.main.transform;

        Vector3 move = BuildCameraRelativeMove(moveInput, cam);
        ApplyMovement(move);
        ApplyFacing(cam, move);
    }

    private Vector3 BuildCameraRelativeMove(Vector2 input, Transform cameraTf)
    {
        Vector3 raw = new Vector3(input.x, 0f, input.y);
        if (raw.sqrMagnitude > 1f) raw.Normalize();

        if (cameraTf == null) return raw;

        Vector3 f = cameraTf.forward;
        Vector3 r = cameraTf.right;
        f.y = 0f;
        r.y = 0f;

        if (f.sqrMagnitude > 0.0001f) f.Normalize();
        if (r.sqrMagnitude > 0.0001f) r.Normalize();

        Vector3 camMove = (f * raw.z + r * raw.x);
        if (camMove.sqrMagnitude > 1f) camMove.Normalize();

        return camMove;
    }

    private void ApplyMovement(Vector3 moveDir)
    {
        if (cc.isGrounded && verticalVel < 0f) verticalVel = -1f;
        verticalVel += gravity * Time.deltaTime;

        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = verticalVel;

        cc.Move(velocity * Time.deltaTime);
    }

    private void ApplyFacing(Transform cameraTf, Vector3 moveDir)
    {
        if (cameraTf == null) return;

        if (faceCameraYaw)
        {
            Vector3 forward = cameraTf.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return;

            Quaternion target = Quaternion.LookRotation(forward.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * turnSpeed);
        }
        else
        {
            if (moveDir.sqrMagnitude < 0.0001f) return;

            Quaternion target = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * turnSpeed);
        }
    }
}
