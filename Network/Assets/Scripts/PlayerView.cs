using TMPro;
using UnityEngine;
using FishNet.Object;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;

    public override void OnStartClient()
    {
        base.OnStartClient();
        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_playerNetwork == null)
            return;

        if (_nicknameText != null)
            _nicknameText.text = _playerNetwork.Nickname.Value;

        if (_hpText != null)
            _hpText.text = $"HP: {_playerNetwork.HP.Value}";
    }
}