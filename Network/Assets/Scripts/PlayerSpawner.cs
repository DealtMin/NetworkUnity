using Unity.Netcode;
using UnityEngine;
//спавн игрока на разных точках
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnpoints;

    private void OnEnable()
    {
        if(NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDisable()
    {
        if(NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    public void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return; //server

        if(_spawnpoints==null || _spawnpoints.Length==0) return; //check null spawn points

        if(!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)) return; //check ckientID is not null

        if (client.PlayerObject == null) return; //проверка что игровой объект не нулевой

        int spawnPointIndex = GetSpawnIndex(clientId);

        Transform spawnPoint = _spawnpoints[spawnPointIndex];

        client.PlayerObject.transform.position = spawnPoint.position;
        client.PlayerObject.transform.rotation = spawnPoint.rotation;
    }

    private int GetSpawnIndex(ulong OwnerClientId)
    {
        if (_spawnpoints == null || _spawnpoints.Length == 0) return 0;

        return Mathf.Clamp((int)OwnerClientId,0,_spawnpoints.Length-1);
    }
}
