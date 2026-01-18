using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpGem : MonoBehaviour
{
    [Header("Setting")]
    public int expAmount = 10;

    private Transform _targetPlayer;
    private bool isMagnet = false;
    private float _magnetSpeed = 15f;
    public void Initialize(Transform player)
    {
        _targetPlayer = player;
        isMagnet = false;
    }
    private void OnEnable()
    {
        Initialize(null);
        isMagnet = false;
    }
    private void Update()
    {
        if(_targetPlayer == null) return;
        
        if (isMagnet)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPlayer.position, _magnetSpeed * Time.deltaTime);
        }
        else if (Vector3.Distance(transform.position, _targetPlayer.position) < 3f) { isMagnet = true; }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(GameManager.Instance != null) GameManager.Instance.GetExp(expAmount);
            gameObject.SetActive(false);
        }
    }
}
