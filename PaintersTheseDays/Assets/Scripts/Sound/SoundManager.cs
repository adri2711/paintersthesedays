using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSourceOneShot;
    private AudioSource audioSourceFootsteps;
    private AudioSource audioSourceAmbient;
    [SerializeField] private AudioClip selectPaintSound;
    [SerializeField] private AudioClip mixPaintSound;
    [SerializeField] private AudioClip brushHitSound;
    [SerializeField] private AudioClip placeCanvasSound;
    [SerializeField] private AudioClip waterSound;
    [SerializeField] private AudioClip waterSplashSound;
    [SerializeField] private AudioClip removeSound;
    [SerializeField] private AudioClip footstepSounds;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip ambientSound;

    Coroutine changeVolumeCoroutine;

    public static SoundManager Instance => _instance;
    private static SoundManager _instance;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        audioSourceOneShot = gameObject.AddComponent<AudioSource>();
        audioSourceFootsteps = gameObject.AddComponent<AudioSource>();
        audioSourceFootsteps.clip = footstepSounds;
        audioSourceFootsteps.volume = 0f;
        audioSourceFootsteps.loop = true;
        audioSourceFootsteps.Play();
        audioSourceAmbient = gameObject.AddComponent<AudioSource>();
        audioSourceAmbient.clip = ambientSound;
        audioSourceAmbient.loop = true;
        audioSourceAmbient.volume = 0.3f;
        audioSourceAmbient.Play();
    }

    public void SelectPaintSound()
    {
        audioSourceOneShot.PlayOneShot(selectPaintSound, 0.9f);
        audioSourceOneShot.PlayOneShot(waterSplashSound, 0.1f);
    }
    public void MixPaintSound()
    {
        audioSourceOneShot.PlayOneShot(mixPaintSound, 0.7f);
        audioSourceOneShot.PlayOneShot(waterSplashSound, 0.1f);
    }
    public void BrushHitSound()
    {
        audioSourceOneShot.PlayOneShot(brushHitSound, 0.1f);
    }
    public void PlaceCanvasSound()
    {
        audioSourceOneShot.PlayOneShot(placeCanvasSound, 0.6f);
    }
    public void WaterSound()
    {
        audioSourceOneShot.PlayOneShot(waterSound, 0.7f);
        audioSourceOneShot.PlayOneShot(waterSplashSound, 0.4f);
    }
    public void WaterSplashSound()
    {
        audioSourceOneShot.PlayOneShot(waterSplashSound);
    }
    public void RemoveSound()
    {
        audioSourceOneShot.PlayOneShot(placeCanvasSound, 0.1f);
        audioSourceOneShot.PlayOneShot(removeSound, 0.2f);
    }
    public void LandSound()
    {
        audioSourceOneShot.PlayOneShot(landSound, .5f);
    }
    public void MuteFootstepSounds()
    {
        if (changeVolumeCoroutine != null) StopCoroutine(changeVolumeCoroutine);
        changeVolumeCoroutine = StartCoroutine(ChangeVolume(audioSourceFootsteps, audioSourceFootsteps.volume, 0f, .25f));
    }
    public void ResumeFootstepSounds()
    {
        if (changeVolumeCoroutine != null) StopCoroutine(changeVolumeCoroutine);
        audioSourceFootsteps.volume = .5f;
    }
    public IEnumerator ChangeVolume(AudioSource source, float initial, float final, float time, int it = 10)
    {
        if (source.volume == final)
        {
            yield return null;
        }
        else
        {
            for (int i = 0; i < it; i++)
            {
                source.volume = Mathf.Lerp(initial, final, (float)i / it);
                yield return new WaitForSeconds(time / it);
            }
            source.volume = final;
        }
    }
}
