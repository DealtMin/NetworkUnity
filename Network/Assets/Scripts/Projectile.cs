using UnityEngine;
using FishNet.Object;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float _speed = 18f;
    [SerializeField] private int _damage = 20;
    [SerializeField] private float _lifetime = 5f;

    private float _spawnTime;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _spawnTime = Time.time;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);

        // Только сервер удаляет снаряд по таймеру
        if (IsServerInitialized && Time.time > _spawnTime + _lifetime)
        {
            Despawn(gameObject);
        }
    }

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

        // Не стреляем в самого себя
        if (target.Owner.ClientId == Owner.ClientId)
            return;

        target.TakeDamage(_damage);

        Despawn(gameObject);
    }
}