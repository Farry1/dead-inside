using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StandardWeapon", menuName = "Weapon/StandardWeapon")]
public class Weapon : ScriptableObject
{
    public int damage;
    public GameObject projectile;

    public void Fire(Node origin, Node target, string hitTag)
    {
        Vector3 direction = target.transform.position - origin.transform.position;

        Debug.DrawRay(origin.transform.position, direction * 5, Color.red, 5f);

        GameObject p = Instantiate(projectile, origin.transform.position + new Vector3(0, 0.5f, 0), Quaternion.LookRotation(direction));
        p.GetComponent<Rigidbody>().velocity = p.transform.forward * 8;
        p.GetComponent<Projectile>().damage = damage;
        p.transform.tag = hitTag;
        p.transform.gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");
    }
}
