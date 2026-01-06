using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Transform _target;
    public float speed = 3.0f;
    private void Start()
    {
        _target = PlayerController.Instance.transform;
    }
    private void Update()
    {
        if (_target == null) return;
        Vector3 dir = (_target.position - transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);
    }
}
