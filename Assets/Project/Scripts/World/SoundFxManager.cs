using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFxManager : MonoBehaviour
{
    public AudioSource sfxAudioSource;
    public AudioSource ambientAudioSource;
    public AudioClip[] sfxClips;
    public AudioClip[] music;
    public AudioClip[] ambient;
    public AudioClip[] snowRun;
    public AudioClip[] sandRun;
    public AudioClip[] walking;
    public AudioClip bulletShot;

    public float runVolume = 0.3f;

    public static SoundFxManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void PlaySfx(int index)
    {
        sfxAudioSource.PlayOneShot(sfxClips[index]);
    }
    
    public void PlayMusic(int index)
    {
        StopAmbient();
        ambientAudioSource.clip = music[index];
        ambientAudioSource.Play();
    }

    public void StopSFX(int index)
    {
        sfxAudioSource.clip = music[index];
        sfxAudioSource.Stop();
    }
    
    public void StopSFX()
    {
        sfxAudioSource.Stop();
    }

    public void StopMusic()
    {
        ambientAudioSource.Stop();
    }

    public void PlayAmbient(int index)
    {
        ambientAudioSource.Stop();
        ambientAudioSource.clip = ambient[index];
        ambientAudioSource.Play();
    }

    public void PlayAmbient()
    {
        PlayAmbient(Random.Range(0, ambient.Length));
    }

    public void StopAmbient()
    {
        ambientAudioSource.Stop();
    }

    public void RunSand()
    {
        if(sfxAudioSource.isPlaying)
        {
            return;
        }

        sfxAudioSource.Stop();
        //Debug.Log("RunSand \n");
        //sfxAudioSource.PlayOneShot(sandRun[Random.Range(0, sandRun.Length)], runVolume);
        sfxAudioSource.PlayOneShot(sandRun[Random.Range(0, sandRun.Length)]);
    }

    public void RunSnow()
    {
        if(sfxAudioSource.isPlaying)
        {
            return;
        }

        sfxAudioSource.Stop();
        //Debug.Log("RunSnow \n");
        //sfxAudioSource.PlayOneShot(snowRun[Random.Range(0, snowRun.Length)], runVolume);
        sfxAudioSource.PlayOneShot(snowRun[Random.Range(0, snowRun.Length)]);
    }

    public void Walk()
    {
        if(sfxAudioSource.isPlaying)
        {
            return;
        }

        sfxAudioSource.Stop();
        //Debug.Log("Walk \n");
        //sfxAudioSource.PlayOneShot(walking, runVolume);
        sfxAudioSource.PlayOneShot(walking[Random.Range(0, walking.Length)]);
    }

    public void Shoot()
    {
        sfxAudioSource.Stop();
        sfxAudioSource.PlayOneShot(bulletShot);
    }
}
