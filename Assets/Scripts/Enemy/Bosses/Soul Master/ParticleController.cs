using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public static ParticleController instance;

    public bool IsParticleOn;
    public Vector2 StartPos;
    public Vector2 EndPos;

    private ParticleSystem m_particleSystem;

    private Coroutine co;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            return;
        }

        m_particleSystem = GetComponent<ParticleSystem>();
    }

    public void PlayParticle()
    {
        m_particleSystem.Play();
    }

    public void StopParticle()
    {
        m_particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }
}
