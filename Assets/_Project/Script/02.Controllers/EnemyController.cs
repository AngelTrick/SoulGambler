using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class EnemyController : MonoBehaviour
{
    [Header("Ω∫≈»")]
    public float maxHP = 10f;
    public float speed = 3.0f;
    private float _currentHP;

    private Rigidbody _rb;
    private Transform _target;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();    
    }
    private void OnEnable()
    {
        _currentHP = maxHP;
        if(_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        if(PlayerController.Instance != null)
        {
            _target = PlayerController.Instance.transform;
        }
    }
    
    private void Update()
    {
        if (_target == null) return;
        Vector3 dir = (_target.position - transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);
    }
    public void TakeDamage(float damage)
    {
        _currentHP -= damage;
        if(_currentHP < 0)
        {
            Dead();
        }
    }
    private void OnMouseDown()
    {
        TakeDamage(100);
    }
    void Dead()
    {
        gameObject.SetActive(false);
    }
}
