using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreStar : MonoBehaviour
{
    public Image star;
    public ParticlePlayer startFX;
    public float delay = 0.5f;
    public AudioClip startSound;
    public bool activeated = false;

    private void Start()
    {
        SetActive(false);
        //StartCoroutine(TestRoutine());
    }

    private void SetActive(bool state)
    {
        if(star != null)
        {
            star.gameObject.SetActive(state);
        }
    }

    public void Activate()
    {
        if(activeated) return;
        StartCoroutine(ActivateRoutine());
    }

    IEnumerator ActivateRoutine()
    {
        activeated = true;
        if(startFX != null)
        {
            Debug.Log("123");
            startFX.Play();
        }
        if(SoundManager.Instance != null && startSound !=null)
        {
            SoundManager.Instance.PlayClipAtPoint(startSound,Vector3.zero, SoundManager.Instance.fxVolume);
        }

        yield return new WaitForSeconds(delay);

        SetActive(true);
    }

    IEnumerator TestRoutine()
    {
        yield return new WaitForSeconds(3f);
        Activate();
    }
}
