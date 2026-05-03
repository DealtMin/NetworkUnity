using FishNet.Object;
using UnityEngine;

public class KillCube : NetworkBehaviour
{
    [SerializeField] private int _damage = 20;


    private void OnTriggerEnter(Collider other)
    {
        // Только сервер обрабатывает попадания
        if (!IsServerInitialized)
            return;

        if (!IsSpawned)
            return;

        PlayerNetwork target = other.GetComponent<PlayerNetwork>();
        if (target == null)
            return;

        int newHp = Mathf.Max(0, target.HP.Value - _damage);
        target.HP.Value = newHp;

    }
}
