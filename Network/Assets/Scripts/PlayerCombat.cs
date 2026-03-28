using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private int _damage = 10;
    [SerializeField] private PlayerInput playerInput;

    void OnEnable()
    {
        playerInput.OnAttackPressed += Attack;
    }
    void OnDisable()
    {
        playerInput.OnAttackPressed -= Attack;
    }
    private void Attack()
    {
        TryAttack(FindWhoAttack());                
    }

    // Пока без красивой реализации кого атаковать
    private PlayerNetwork FindWhoAttack()
    {
        PlayerNetwork[] players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player != _playerNetwork)
            return player;
        }
        return _playerNetwork;
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
        {
            Debug.Log("Can't attack yourself");
            return;
        }

        // Итоговое значение HP ограничиваем снизу нулем.
        int nextHp = Mathf.Max(0, targetPlayer.HP.Value - damage);
        targetPlayer.HP.Value = nextHp;
    }
}