using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int MaxHealth;
    [SerializeField] private int CurrentHealth;
    public int Damage;

    private bool m_isWorking = false;

    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (!m_isWorking) return;

        if (PatternOneOn())
        {

        } else if (PatternTwoOn())
        {

        } else if (PatternThreeOn())
        {

        } else
        {
            Idle();
        }
    }

    protected virtual bool PatternOneOn()
    {
        return false;
    }

    protected virtual bool PatternTwoOn()
    {
        return false;
    }

    protected virtual bool PatternThreeOn()
    {
        return false;
    }

    protected virtual void PatternOne()
    {

    }

    protected virtual void PatternTwo()
    {

    }

    protected virtual void PatternThree()
    {

    }

    private void Idle()
    {

    }

    private void OnBecameInvisible()
    {
        m_isWorking = false;
    }

    private void OnBecameVisible()
    {
        m_isWorking = true;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        
        if (CurrentHealth <= 0)
        {
            Dead();
        }
    }

    private void Dead()
    {
        //drop geo
        gameObject.SetActive(false);
    }

    //Vector3.Reflect() - for boss pattern
    //Mathf.Repeat()
}
