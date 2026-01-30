using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType {Ranged, Melee}
public enum ItemGrade {Normal, Rare, Epic , Legendary }
[CreateAssetMenu(fileName = "NewWeapon",menuName = "SO/Weapon")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName;
    public WeaponType weaponType;
    public ItemGrade grade; 
    [TextArea] public string description;
    public Sprite icon;

    [Header("Base Stats")]
    public float baseDamage;
    public float attackSpeed;
    public float range;
    public float projectileSpeed;

    [Header("RPG Stat")]
    public int amount = 1;
    public int pierce = 0;
    public float knockback = 0f;

    [Header("Prefab")]
    public GameObject projectilePrefab;
}
