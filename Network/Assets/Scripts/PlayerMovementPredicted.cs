using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public struct MoveData : IReplicateData
{
    public float Horizontal;
    public float Vertical;
    public bool Jump;

    private uint _tick;

    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
    public void Dispose() { }
}

public struct ReconcileData : IReconcileData
{
    public Vector3 Position;
    public Vector3 Velocity;

    private uint _tick;

    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
    public void Dispose() { }
}

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementPredicted : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    private CharacterController _controller;
    private PlayerNetwork _playerNetwork;
    private Vector3 _velocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerNetwork = GetComponent<PlayerNetwork>();
    }


public override void CreateReconcile()
{
    ReconcileData rd = new ReconcileData
    {
        Position = transform.position,
        Velocity = _velocity
    };

    Reconcile(rd);
}
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        TimeManager.OnTick += TimeManager_OnTick;
        TimeManager.OnPostTick += TimeManager_OnPostTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        TimeManager.OnTick -= TimeManager_OnTick;
        TimeManager.OnPostTick -= TimeManager_OnPostTick;
    }

    private void TimeManager_OnTick()
    {
        if (!IsOwner && !IsServerInitialized)
            return;

        MoveData md = new MoveData();

        if (IsOwner)
        {
            md.Horizontal = Input.GetAxisRaw("Horizontal");
            md.Vertical = Input.GetAxisRaw("Vertical");
            md.Jump = Input.GetButton("Jump");
        }

        Replicate(md);
    }

    private void TimeManager_OnPostTick()
    {
        if (IsServerInitialized)
        {
            ReconcileData rd = new ReconcileData
            {
                Position = transform.position,
                Velocity = _velocity
            };

            Reconcile(rd);
        }
    }

    [Replicate]
    private void Replicate(
        MoveData data,
        ReplicateState state = ReplicateState.Invalid,
        Channel channel = Channel.Unreliable)
    {
        if (_playerNetwork != null && !_playerNetwork.IsAlive.Value)
            return;

        if (_controller == null || !_controller.enabled)
            return;

        float delta = (float)TimeManager.TickDelta;

        Vector3 move = new Vector3(data.Horizontal, 0f, data.Vertical).normalized;
        move = transform.TransformDirection(move);

        _controller.Move(move * moveSpeed * delta);

        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        if (data.Jump && _controller.isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * delta;
        _controller.Move(_velocity * delta);
    }

    [Reconcile]
    private void Reconcile(
        ReconcileData data,
        Channel channel = Channel.Unreliable)
    {
        if (_controller != null && _controller.enabled)
        {
            _controller.enabled = false;

            transform.position = data.Position;
            _velocity = data.Velocity;

            _controller.enabled = true;
        }
        else
        {
            transform.position = data.Position;
            _velocity = data.Velocity;
        }
    }

    public void ResetVelocity()
    {
        _velocity = Vector3.zero;
    }
}