using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    private PlayerInput _playerInput;
    public static PlayerController Instance;
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        Instance = this;
    }
    private void Update()
    {
        Vector2 inputVec = _playerInput.actions["Move"].ReadValue<Vector2>();
        Vector3 moveDir = new Vector3(inputVec.x, 0,inputVec.y );
        transform.Translate(moveDir * speed * Time.deltaTime);
    }
}
