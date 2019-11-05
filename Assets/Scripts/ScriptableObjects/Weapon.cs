using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StandardWeapon", menuName = "Weapon/StandardWeapon")]
public class Weapon : ScriptableObject
{
   
    public enum ProjectileType { Linear, Areal }
    [Header("Projetile Settings")]
    public GameObject projectile;
    public ProjectileType projetileType;
    public int projetilePushAmount;
    

    [Header("Weapon Settings")]
    public int damage;
    public int recoilAmount = 0;
    [Range(0, 99)] public float range = 3;

    public List<Action> attachedActions = new List<Action>();
}
