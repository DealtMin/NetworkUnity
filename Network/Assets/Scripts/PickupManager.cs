using Unity.Netcode;
using UnityEngine;
using System.Collections;

// PickupManager — обычный MonoBehaviour, работает только на сервере
public class PickupManager : MonoBehaviour
{
    [SerializeField] private GameObject _healthPickupPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 10f;
    
    private bool _isInitialized = false;

    private void Start()
    {
        // Подписываемся на событие запуска сервера
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            
            // Если сервер уже запущен - вызываем сразу
            if (NetworkManager.Singleton.IsServer)
            {
                OnServerStarted();
            }
        }
        else
        {
            Debug.LogError("[PickupManager] NetworkManager not found!");
        }
    }

    private void OnServerStarted()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        
        Debug.Log("[PickupManager] Server started event received, spawning pickups...");
        SpawnAll();
    }

    private void SpawnAll()
    {
        foreach (var point in _spawnPoints)
        {
            if (point != null)
            {
                SpawnPickup(point.position);
            }
        }
    }

    public void OnPickedUp(Vector3 position)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        StartCoroutine(RespawnAfterDelay(position));
    }

    private IEnumerator RespawnAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(_respawnDelay);
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            SpawnPickup(position);
        }
    }

    private void SpawnPickup(Vector3 position)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("[PickupManager] Cannot spawn - not server");
            return;
        }
        
        var go = Instantiate(_healthPickupPrefab, position, Quaternion.identity);
        var pickup = go.GetComponent<HealthPickup>();
        var netObj = go.GetComponent<NetworkObject>();
        
        if (pickup != null && netObj != null)
        {
            pickup.Init(this);
            netObj.Spawn();
            Debug.Log($"[PickupManager] Spawned pickup at {position}");
        }
        else
        {
            Debug.LogError("[PickupManager] Failed to spawn - missing components");
            Destroy(go);
        }
    }
    
    private void OnDestroy()
    {
        // Отписываемся от события
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }
}
