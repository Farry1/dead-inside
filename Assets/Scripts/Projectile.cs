using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [HideInInspector] public GameObject target;
    [HideInInspector] public int damage;
    [HideInInspector] public int projectilePushAmount;
    [HideInInspector] public Weapon.ProjectileType projectileType;
    [HideInInspector] public Node hitNode;
    [HideInInspector] public Unit targetUnit;
    [HideInInspector] public Unit initialUnit;
    [HideInInspector] public Vector3 hitDirection;
    [HideInInspector] public Node targetPushbackNode;

    public GameObject hitFX;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "ProjectileTarget" && collision.gameObject == target)
        {
            Destroy(target.gameObject);
            GameObject hitFXInstance = Instantiate(hitFX, collision.contacts[0].point, Quaternion.identity);

            if (targetUnit != null)
            {
                targetUnit.healthController.Damage(damage);
                if (projectileType == Weapon.ProjectileType.Linear)
                {
                    if (targetPushbackNode != null)
                    {
                        targetUnit.StartCoroutine(targetUnit.unitMovement.MoveWithPush(projectilePushAmount, hitDirection));
                    }
                    else
                    {
                        targetUnit.StartCoroutine(targetUnit.unitMovement.DieLonesomeInSpace(hitDirection));
                    }
                }
                else if (projectileType == Weapon.ProjectileType.Areal)
                {
                    targetUnit.currentNode.PushAdjacentUnits(projectilePushAmount);
                }
            }

            if (hitNode != null)
            {
                if (projectileType == Weapon.ProjectileType.Areal)
                {
                    hitNode.PushAdjacentUnits(projectilePushAmount);
                }
            }

            Debug.Log("Target Hit!");
            Destroy(this.gameObject);
        }
    }
}

