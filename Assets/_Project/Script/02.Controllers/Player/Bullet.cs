using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private Vector3 _direction;

    public void Init(WeaponDataSO data, float damageMultiplier, Vector3 dir)
    {
        _damage = data.baseDamage * damageMultiplier;
        _speed = data.projectileSpeed;
        _direction = dir.normalized;

        if(_direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(_direction);
            transform.rotation = Quaternion.Euler(90, lookRot.eulerAngles.y, 0);
        }

        CancelInvoke(nameof(Despawn));
        Invoke(nameof(Despawn), 3f);
    }
    private void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if(enemy != null)
            {
                enemy.TakeDamage(_damage);
            }

            Despawn();
        }
    }
    private void Despawn()
    {
        gameObject.SetActive(false);
    }
}
