using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Component.Transforming;
using System.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    public readonly SyncVar<string> Nickname = new SyncVar<string>();
    public readonly SyncVar<int> HP = new SyncVar<int>(100);
    public readonly SyncVar<bool> IsAlive = new SyncVar<bool>(true);

    [SerializeField] private Respawn[] _spawnPoints;
    [SerializeField] private GameObject _playerVisual;

    private bool _isRespawning;
    private Coroutine _respawnCoroutine;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        _spawnPoints = FindObjectsByType<Respawn>(FindObjectsSortMode.None);

        // Подписка на SyncVar.
        IsAlive.OnChange += OnAliveChanged;
        HP.OnChange += OnHPChanged;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        IsAlive.OnChange -= OnAliveChanged;
        HP.OnChange -= OnHPChanged;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);

        UpdateVisual(IsAlive.Value);
    }

    private void OnDestroy()
    {
        if (_respawnCoroutine != null)
        {
            StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitNicknameServerRpc(string nickname)
    {
        int id = Owner != null ? Owner.ClientId : -1;

        Nickname.Value = string.IsNullOrWhiteSpace(nickname)
            ? $"Player_{id}"
            : nickname.Trim();
    }

    private void Update()
    {
        // Логика смерти только на сервере.
        if (!IsServerStarted)
            return;

        HandleDeath();
    }

    private void HandleDeath()
    {
        if (HP.Value <= 0 && IsAlive.Value && !_isRespawning)
        {
            IsAlive.Value = false;
            _isRespawning = true;

            _respawnCoroutine = StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        PerformRespawn();

        _isRespawning = false;
        _respawnCoroutine = null;
    }

    private void PerformRespawn()
    {
        if (!IsServerStarted)
            return;

        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            int idx = Random.Range(0, _spawnPoints.Length);
            Vector3 spawnPos = _spawnPoints[idx].transform.position;

            CharacterController cc = GetComponent<CharacterController>();
            NetworkTransform nt = GetComponent<NetworkTransform>();

            if (cc != null)
            {
                cc.enabled = false;
                transform.position = spawnPos;
                nt.Teleport();
                cc.enabled = true;
            }
            else
            {
                transform.position = spawnPos;
                nt.Teleport();
            }
        }

        HP.Value = 100;
        IsAlive.Value = true;
    }

    private void OnAliveChanged(bool prev, bool next, bool asServer)
    {
        // Вызывается у всех клиентов и сервера.
        UpdateVisual(next);
    }

    private void OnHPChanged(int prev, int next, bool asServer)
    {
        // UI hp bar и т.п.
    }

    private void UpdateVisual(bool isAlive)
    {
        if (_playerVisual != null)
            _playerVisual.SetActive(isAlive);

        if (TryGetComponent<Collider>(out var col))
            col.enabled = isAlive;

        if (TryGetComponent<CharacterController>(out var cc))
            cc.enabled = isAlive;
    }
}