using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private Vector3 _direction;

    private int _pierceCount;
    private float _knockBack;
    private Coroutine _despawnCoroutine;

    public void Init(WeaponDataSO data, float damageMultiplier, Vector3 dir,
        int bonusPierce = 0, float bonusKnockback = 0f, float areaScale = 1.0f)
    {
        _damage = data.baseDamage * damageMultiplier;
        _speed = data.projectileSpeed;
        _direction = dir.normalized;

        _pierceCount = data.pierce + bonusPierce;
        _knockBack = data.knockback + bonusKnockback;

        transform.localScale = Vector3.one * areaScale;

        if(_direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(_direction);
            transform.rotation = Quaternion.Euler(90, lookRot.eulerAngles.y, 0);
        }

        if (_despawnCoroutine != null) StopCoroutine(_despawnCoroutine);
        _despawnCoroutine = StartCoroutine(CoDespawn(3f));
    }
    private void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") == false) return;
        if(other.TryGetComponent(out EnemyController enemy))
        {
            enemy.TakeDamage(_damage);
            if(_knockBack > 0)
            {
                enemy.KnockBack(_direction, _knockBack);
            }
        }
            
        if (_pierceCount > 0)
        {
           _pierceCount--;
            return;
        }
        
        Despawn();
    }
    private IEnumerator CoDespawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn();
    }
    private void Despawn()
    {
        gameObject.SetActive(false);
    }
}
