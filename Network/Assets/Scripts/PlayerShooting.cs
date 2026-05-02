using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _cooldown = 0.4f;
    [SerializeField] private int _maxAmmo = 10;

    private float _lastShotTime;
    private int _currentAmmo;

    private PlayerNetwork _playerNetwork;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (!_playerNetwork.IsAlive.Value)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootServerRpc(_firePoint.position, _firePoint.forward);
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir, NetworkConnection sender = null)
    {
        if (_playerNetwork.HP.Value <= 0)
            return;

        if (Time.time < _lastShotTime + _cooldown)
            return;

        _lastShotTime = Time.time;

        GameObject go = Instantiate(
            _projectilePrefab,
            pos + dir * 1.2f,
            Quaternion.LookRotation(dir)
        );

        var networkObject = go.GetComponent<NetworkObject>();
        InstanceFinder.ServerManager.Spawn(networkObject, sender);
    }
}