using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public enum AttackMode { Auto, SemiAuto, Manual}
public class WeaponSystem : MonoBehaviour
{
    [Header("Equipped Weapon")]
    public WeaponDataSO currentWeapon;
    [Header("Setup")]
    public Transform firePoint;
    [Header("Mode Setting")]
    public AttackMode currentMode = AttackMode.Auto;

    private float _timer = 0f;
    private Transform _target;
    private PlayerInput _playerInput;
    private bool _isFiringPressed = false;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (currentWeapon == null || PlayerController.Instance.IsDead) return;
        if(_playerInput != null)
        {
            try
            {
                _isFiringPressed = _playerInput.actions["Attack"].IsPressed();
            }
            catch 
            {
                _isFiringPressed = Mouse.current.leftButton.isPressed;
            }
        }
        _timer += Time.deltaTime;
        float cooldown = 1f / currentWeapon.attackSpeed;

        if(_timer >= cooldown)
        {
            if (ShouldFire())
            {
                UpdateTargeting();
                if(_target != null || currentMode == AttackMode.Manual)
                {
                    Fire();
                    _timer = 0f;
                }
            }
        }
    }

    bool ShouldFire()
    {
        switch (currentMode)
        {
            case AttackMode.Auto:
                return true;
            case AttackMode.SemiAuto:
            case AttackMode.Manual:
                return _isFiringPressed;

            default: return false;
        }
    }

    void UpdateTargeting()
    {
        if(currentMode == AttackMode.Manual)
        {
            _target = null;
        }
        else 
        {
            FindNearestEnemy();
        }
    }
    void FindNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, currentWeapon.range);

        Transform nearest = null;
        float minDistance = Mathf.Infinity;
        foreach (Collider col in enemies)
        {
            if (col.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if(dist < minDistance)
                {
                    minDistance = dist;
                    nearest = col.transform;
                }
            }
        }
        _target = nearest;
    }

    void Fire()
    {
        if (currentWeapon.projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        Vector3 dir = Vector3.forward;

        if (currentMode == AttackMode.Manual)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                dir = sr.flipX ? Vector3.left : Vector3.right;
            }
        }
        else if (_target != null)
        {
            dir = (_target.position - spawnPos).normalized;
            dir.y = 0;
        }
        else return;

        GameObject bulletObj = Instantiate(currentWeapon.projectilePrefab, spawnPos, Quaternion.identity);

        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if(bulletScript != null)
        {
            float damageMultiplier = (PlayerController.Instance.currentStance == PlayerStance.Dark) ? 1.5f : 1.0f;
            bulletScript.Init(currentWeapon, damageMultiplier, dir);
        }
    }
    public void CycleMode()
    {
        int nextMode = (int)currentMode + 1;
        if (nextMode > (int)AttackMode.Manual) nextMode = 0;
        currentMode = (AttackMode)nextMode;

        Debug.Log($"공격 모드 변경 : {currentMode}");
    }
    private void OnDrawGizmosSelected()
    {
        if(currentWeapon != null) 
        {
            Gizmos.color = currentMode == AttackMode.Auto ? Color.yellow : Color.green;
            Gizmos.DrawWireSphere(transform.position, currentWeapon.range);
        }
    }
}
