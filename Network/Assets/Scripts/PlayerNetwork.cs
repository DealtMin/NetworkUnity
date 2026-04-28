using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    // Ник должен быть виден всем клиентам, но менять его может только сервер.
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // HP тоже читает каждый клиент, но изменяется только на сервере.
    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private GameObject _playerVisual;
    
    // Флаг для предотвращения множественных корутин
    private bool _isRespawning = false;
    private Coroutine _respawnCoroutine;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
        
        // Подписываемся на изменения HP и IsAlive
        HP.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;
        
        // Сразу применяем текущее состояние визуала
        UpdateVisualState(IsAlive.Value);
    }

    public override void OnNetworkDespawn()
    {
        // Отписываемся от событий при уничтожении объекта
        HP.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
        
        // Останавливаем корутину, если она запущена
        if (_respawnCoroutine != null)
        {
            StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = null;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }
    
    private void OnHpChanged(int prev, int next)
    {
        // Только сервер обрабатывает смерть
        if (!IsServer) return;
        
        // Если HP упало до 0 или ниже, игрок ещё жив, и респавн ещё не запущен
        if (next <= 0 && IsAlive.Value && !_isRespawning)
        {
            IsAlive.Value = false;
            _isRespawning = true;
            
            // Запускаем корутину только если GameObject активен
            if (gameObject.activeInHierarchy)
            {
                _respawnCoroutine = StartCoroutine(RespawnRoutine());
            }
            else
            {
                // Если объект неактивен, используем альтернативный подход
                Debug.LogWarning($"Player {OwnerClientId} is inactive, using delayed respawn via Invoke");
                Invoke(nameof(RespawnViaInvoke), 3f);
            }
        }
    }
    
    private IEnumerator RespawnRoutine()
    {
        // Ждём 3 секунды
        yield return new WaitForSeconds(3f);
        
        // Выполняем респавн
        PerformRespawn();
        
        _isRespawning = false;
        _respawnCoroutine = null;
    }
    
    private void RespawnViaInvoke()
    {
        PerformRespawn();
        _isRespawning = false;
    }
    
    private void PerformRespawn()
    {
        if (!IsServer) return;
    
        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            int idx = Random.Range(0, _spawnPoints.Length);
        
            // Прямая работа с CharacterController
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                transform.position = _spawnPoints[idx].position;
                cc.enabled = true;
            }
            else
            {
                transform.position = _spawnPoints[idx].position;
            }
        }
        else
        {
            Debug.LogWarning("Нет точек спавна! Игрок возрождается на месте.");
        }
    
        HP.Value = 100;
        IsAlive.Value = true;
    }
    
    private void OnIsAliveChanged(bool prev, bool next)
    {
        // Это выполняется на всех клиентах при изменении IsAlive
        UpdateVisualState(next);
    }
    
    private void UpdateVisualState(bool isAlive)
    {
        // Показываем или скрываем визуальную модель
        if (_playerVisual != null)
        {
            _playerVisual.SetActive(isAlive);
        }
        
        // Если игрок умер — отключаем коллайдер и CharacterController
        if (TryGetComponent<Collider>(out var col))
        {
            col.enabled = isAlive;
        }
        
        if (TryGetComponent<CharacterController>(out var cc))
        {
            cc.enabled = isAlive;
        }
        
        // Включаем/выключаем GameObject в зависимости от состояния
        //gameObject.SetActive(isAlive) вызовет проблемы с корутинами
    }
}