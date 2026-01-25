using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon",menuName = "SO/Weapon")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Info")]
    public string weaponName;
    [TextArea] public string descrption;
    public Sprite icon;

    [Header("Stats")]
    public float baseDamage;
    public float attackSpeed;
    public float range;
    public float projectileSpeed;

    [Header("Prefab")]
    public GameObject projectilePrefab;
}
