using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float _speed = 18f;
    [SerializeField] private int _damage = 20;
    [SerializeField] private float _lifetime = 5f; // добавим время жизни
    
    private float _spawnTime;
    
    public override void OnNetworkSpawn()
    {
        _spawnTime = Time.time;
    }
    
    private void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
        
        // Уничтожаем снаряд через 5 секунд (только на сервере)
        if (IsServer && Time.time > _spawnTime + _lifetime)
        {
            NetworkObject.Despawn(destroy: true);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // ТОЛЬКО СЕРВЕР обрабатывает попадания
        if (!IsServer) return;
        
        // Ждём один кадр после спавна, чтобы избежать ошибки
        if (!IsSpawned) return;
        
        var target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;
        
        // Не наносим урон самому себе
        if (target.OwnerClientId == OwnerClientId) return;
        
        int newHp = Mathf.Max(0, target.HP.Value - _damage);
        target.HP.Value = newHp;
        
        // Делаем Despawn только если объект уже заспавнен
        NetworkObject.Despawn(destroy: true);
    }
}