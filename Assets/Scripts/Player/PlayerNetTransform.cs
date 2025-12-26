// PlayerNetTransform.cs
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public sealed class PlayerNetTransform : MonoBehaviourPun, IPunObservable
{
    [Header("Send rules")]
    [SerializeField] private float positionSendThreshold = 0.15f;
    [SerializeField] private float rotationSendThreshold = 2.5f;
    [SerializeField] private float sendInterval = 0.1f;

    [Header("Remote smoothing")]
    [SerializeField] private float remoteLerpSpeed = 12f;

    private Vector3 targetPos;
    private Quaternion targetRot;

    private Vector3 lastSentPos;
    private Quaternion lastSentRot;
    private float lastSendTime;

    private void Awake()
    {
        targetPos = transform.position;
        targetRot = transform.rotation;

        lastSentPos = transform.position;
        lastSentRot = transform.rotation;
        lastSendTime = 0f;
    }

    private void Update()
    {
        if (photonView.IsMine) return;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * remoteLerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * remoteLerpSpeed);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            bool canSend = (Time.time - lastSendTime) >= sendInterval;

            bool sendPos = false;
            bool sendRot = false;

            if (canSend)
            {
                sendPos = Vector3.Distance(transform.position, lastSentPos) >= positionSendThreshold;
                sendRot = Quaternion.Angle(transform.rotation, lastSentRot) >= rotationSendThreshold;

                if (sendPos || sendRot) lastSendTime = Time.time;
            }

            stream.SendNext(sendPos);
            if (sendPos)
            {
                Vector3 p = transform.position;
                stream.SendNext(p);
                lastSentPos = p;
            }

            stream.SendNext(sendRot);
            if (sendRot)
            {
                Quaternion r = transform.rotation;
                stream.SendNext(r);
                lastSentRot = r;
            }
        }
        else
        {
            bool sendPos = (bool)stream.ReceiveNext();
            if (sendPos) targetPos = (Vector3)stream.ReceiveNext();

            bool sendRot = (bool)stream.ReceiveNext();
            if (sendRot) targetRot = (Quaternion)stream.ReceiveNext();
        }
    }

    public void SnapTo(Vector3 pos, Quaternion rot)
    {
        targetPos = pos;
        targetRot = rot;

        transform.SetPositionAndRotation(pos, rot);

        lastSentPos = pos;
        lastSentRot = rot;
        lastSendTime = 0f;
    }
}
