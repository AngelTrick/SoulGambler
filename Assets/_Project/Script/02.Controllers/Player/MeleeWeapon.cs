using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    private float _damage;
    private bool _isCritical;
    private float _knockBack;

    public void Init(float damage, bool isCritical , float knockback)
    {
        _damage = damage;
        _isCritical = isCritical;
        _knockBack = knockback;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if(other.TryGetComponent(out EnemyController enemy))
            {
                enemy.TakeDamage(_damage, _isCritical);

                if(_knockBack > 0)
                {
                    Vector3 dir = (other.transform.position - transform.position).normalized;
                    enemy.KnockBack(dir, _knockBack);
                }
            }
        }
    }
}
