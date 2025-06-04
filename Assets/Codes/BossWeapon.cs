using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWeapon : MonoBehaviour
{
    public int attackDamage = 1;
    public int enragedAttackDamage = 2;

    public Vector3 attackOffset;
    public float attackRange = 1f;
    public LayerMask attackMask;

    private Vector3 GetAttackPosition()
    {
        Vector3 pos = transform.position;
        pos += Vector3.right * attackOffset.x * transform.localScale.x;
        pos += Vector3.up * attackOffset.y;
        return pos;
    }

    public void Attack()
    {
        Vector3 pos = GetAttackPosition();
        Collider2D colInfo = Physics2D.OverlapCircle(pos, attackRange, attackMask);
        if (colInfo != null)
        {
            HealthManager health = colInfo.GetComponent<HealthManager>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }
        }
    }

    public void EnragedAttack()
    {
        Vector3 pos = GetAttackPosition();
        Collider2D colInfo = Physics2D.OverlapCircle(pos, attackRange, attackMask);
        if (colInfo != null)
        {
            HealthManager health = colInfo.GetComponent<HealthManager>();
            if (health != null)
            {
                health.TakeDamage(enragedAttackDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        pos += Vector3.right * attackOffset.x * transform.localScale.x;
        pos += Vector3.up * attackOffset.y;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, attackRange);
    }
}
