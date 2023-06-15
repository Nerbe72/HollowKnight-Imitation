using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator m_attackAnimator;

    [HideInInspector] public bool m_isHit = false;

    private void Awake()
    {
        m_attackAnimator = GetComponent<Animator>();
    }

    public void ChangeAttackCollider()
    {
        m_attackAnimator.SetTrigger("Attack");
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;
        if (collider.CompareTag(GameTagMask.Tag(Tags.Ground))) return;

        if (!m_isHit)
            m_isHit = true;

        if (collider.CompareTag(GameTagMask.Tag(Tags.Enemy)))
        {
            collider.GetComponent<Enemy>().TakeDamage(PlayerStats.instance.Damage);
            PlayerStats.instance.CurrentSoul += 10;
        }

        if (collider.CompareTag(GameTagMask.Tag(Tags.Boss)))
        {
            collider.GetComponent<Boss>().TakeDamage(PlayerStats.instance.Damage);
            PlayerStats.instance.CurrentSoul += 10;
        }
    }
}
