using Unity.Netcode;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private int _damage = 10;


    void Update()
{
    if (!IsOwner) return; // Только локальный игрок может инициировать атаку

    if (Input.GetKeyDown(KeyCode.Space))
    {
        // Простой поиск цели: находим первый объект с PlayerNetwork, который не мы
        PlayerNetwork[] allPlayers = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            if (player != _playerNetwork) // _playerNetwork ссылка на свой компонент
            {
                TryAttack(player);
                break; // Атакуем только первого найденного
            }
        }
    }
}

    public void TryAttack(PlayerNetwork target)
    {
        // Атаку инициирует только локальный владелец объекта.
        if (!IsOwner || target == null)
            return;

        DealDamageServerRpc(target.NetworkObjectId, _damage);
    }

    [ServerRpc]
    private void DealDamageServerRpc(ulong targetObjectId, int damage)
    {
        // Сервер проверяет, существует ли цель среди заспавненных сетевых объектов.
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObject))
            return;

        PlayerNetwork targetPlayer = targetObject.GetComponent<PlayerNetwork>();
        // Запрещаем урон самому себе и удары по некорректной цели.
        if (targetPlayer == null || targetPlayer == _playerNetwork)
            return;

        // Итоговое значение HP ограничиваем снизу нулем.
        int nextHp = Mathf.Max(0, targetPlayer.HP.Value - damage);
        targetPlayer.HP.Value = nextHp;
    }
}