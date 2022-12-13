using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public AudioClip[] musicCilps;
    public AudioClip[] winClips;
    public AudioClip[] loseClips;
    public AudioClip[] bonusClips;

    [Range(0f,1f)]
    public float musicVolume = 0.5f;

    [Range(0f,1f)]
    public float fxVolume = 0.5f;

    public float lowPitch = 0.95f;
    public float highPitch = 1.05f;

    private void Start()
    {
        PlayRandomMusic();
    }

    public AudioSource PlayClipAtPoint(AudioClip clip,Vector3 position, float volume = 1f, bool isRandomPitch = true)
    {
        if(clip != null)
        {
            GameObject go = new GameObject($"SoundFx{clip.name}");
            go.transform.position = position;

            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;

            if(isRandomPitch)
            {
                float randomPitch = Random.Range(lowPitch,highPitch);
                source.pitch = randomPitch;
            }

            source.volume = volume;

            source.Play();
            Destroy(go,clip.length);
            return source;
        }
        return null;
    }

    public AudioSource PlayRandom(AudioClip[] clips,Vector3 position,float volume = 1f)
    {
        if(clips != null)
        {
            if(clips.Length > 0)
            {
                int randomIndex = Random.Range(0,clips.Length);
                if(clips[randomIndex] != null)
                {
                    AudioSource source = PlayClipAtPoint(clips[randomIndex],position,volume);
                    return source;

                }
            }
        }
        return null;
    }

    public void PlayRandomMusic()
    {
        PlayRandom(musicCilps,Vector3.zero, musicVolume);
    }

    public void PlayWinSound()
    {
        PlayRandom(winClips,Vector3.zero,musicVolume);
    }

    public void PlayLoseSound()
    {
        PlayRandom(loseClips,Vector3.zero,musicVolume*0.5f);
    }

    public void PlayBonusSound()
    {
        PlayRandom(bonusClips,Vector3.zero, musicVolume*1.3f);
    }

}