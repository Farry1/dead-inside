using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StandardWeapon", menuName = "Weapon/StandardWeapon")]
public class Weapon : ScriptableObject
{
    public int damage;
    public int maxAmmo = 3;
    public int currentAmmo;
    public int recoil = 0;

    [Range(0, 99)] public float range = 3;
    public GameObject projectile;
    public List<Action> attachedActions = new List<Action>();
}
