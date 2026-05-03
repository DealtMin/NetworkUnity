using UnityEngine;
using FishNet.Object;
using FishNet.Component.Transforming;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private PlayerNetwork _playerNetwork;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerNetwork = GetComponent<PlayerNetwork>();
    }
public void ResetVelocity()
{
    _velocity = Vector3.zero;
}

    private void Update()
    {
        if (!IsOwner)
            return;

        if (_playerNetwork != null && !_playerNetwork.IsAlive.Value)
            return;
        if (_controller.enabled)
            HandleMovement();
    }

    private void HandleMovement()
    {
        // Проверка земли
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        _controller.Move(move * moveSpeed * Time.deltaTime);

        // Гравитация
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}