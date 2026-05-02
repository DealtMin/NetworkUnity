using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerNetwork : NetworkBehaviour
{
    public readonly SyncVar<string> Nickname = new SyncVar<string>();
    public readonly SyncVar<int> HP = new SyncVar<int>(100);
    public readonly SyncVar<bool> IsAlive = new SyncVar<bool>(true);

    [SerializeField] private Respawn[] _spawnPoints;
    [SerializeField] private GameObject _playerVisual;

    public override void OnStartNetwork()
    {
        _spawnPoints = FindObjectsByType<Respawn>(FindObjectsSortMode.None);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitNicknameServerRpc(string nickname)
    {
        int id = Owner != null ? Owner.ClientId : -1;

        Nickname.Value =
            string.IsNullOrWhiteSpace(nickname)
            ? $"Player_{id}"
            : nickname.Trim();
    }

    private void Update()
    {
        if (IsAlive.Value == false)
        {
            if (_playerVisual != null)
                _playerVisual.SetActive(false);
        }
    }
}