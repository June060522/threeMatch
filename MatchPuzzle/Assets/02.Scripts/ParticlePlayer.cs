using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    public ParticleSystem[] allParticles;
    public float lifeTime = 1f;
    public bool desrtoyImmediately = true;
    private void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
        if(desrtoyImmediately) Destroy(gameObject,lifeTime);
    }        

    public void Play()
    {
        foreach (ParticleSystem p in allParticles)
        {
            p.Stop();
            p.Play();
        }
        Destroy(gameObject,lifeTime);
    }
}
