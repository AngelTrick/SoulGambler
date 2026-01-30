using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "DefaultPlayerData", menuName = "SO/PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    [Header("±âº» Á¤º¸")]
    public string jobName;
    [Header("±âº» ½ºÅÝ")]
    public float maxHP = 100f;
    public float defense = 0f;
    public float moveSpeed = 5.0f;
    [Header("ÄÄºª ½ºÅÝ")]
    public float damage = 10f;
    public float critChance = 5f;
    public float critDamage = 1.5f;

    [Header("À¯Æ¿ ½ºÅÝ")]
    public float magnet = 2.0f;
    public float luck = 1.0f;
    [Header("½ºÅ³ ¼¼ÆÃ")]
    public float dashDuration = 0.2f;
    public float dashCooldown = 2.0f;
    public float attackCooldown = 0.5f;

    public float dashSpeed => moveSpeed * 3.0f;


}
