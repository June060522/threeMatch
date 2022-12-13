using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timeLeftText;
    public Image clockImage;

    int m_maxTime = 60;
    public bool paused = false;

    public int flashTimeLimit = 10;
    public AudioClip flashBeep;
    public float flashInterval = 1;
    public Color flashColor = Color.red;
    IEnumerator m_flashRoutine;

    public void InitTimer(int maxTime = 60)
    {
        m_maxTime = maxTime;
        if(clockImage != null)
        {
            clockImage.type = Image.Type.Filled;
            clockImage.fillMethod = Image.FillMethod.Radial360;
            clockImage.fillOrigin = (int)Image.Origin360.Top;
        }
        if(timeLeftText != null)
        {
            timeLeftText.text = $"{maxTime}";
        }
    }

    public void UpdateTimer(int currentTime)
    {
        if(paused) return;
        if(clockImage != null)
        {
            clockImage.fillAmount = (float)currentTime/(float)m_maxTime;
            if(currentTime <= flashTimeLimit)
            {
                m_flashRoutine = FlashRoutine(clockImage, flashColor, flashInterval);
                StartCoroutine(m_flashRoutine);
                SoundManager.Instance.PlayClipAtPoint(flashBeep, Vector3.zero, SoundManager.Instance.fxVolume);
            }
        }
        if(timeLeftText != null)
        {
            timeLeftText.text = currentTime.ToString();
        }
    }
    IEnumerator FlashRoutine(Image image, Color targetColor, float interval)
    {
        if(image != null)
        {
            Color originalColor = image.color;
            image.CrossFadeColor(targetColor,interval * 0.3f, true,true);
            yield return new WaitForSeconds(interval * 0.5f);

            image.CrossFadeColor(originalColor, interval * 0.3f, true,true);
            yield return new WaitForSeconds(interval * 0.5f);
        }
    }

    public void FadeOff()
    {
        if(m_flashRoutine != null)
            StopCoroutine(m_flashRoutine);
        ScreenFader[] screenFaders = GetComponents<ScreenFader>();
        foreach (ScreenFader fader in screenFaders)
        {
            fader.FadeOff();
        }
    }
}
