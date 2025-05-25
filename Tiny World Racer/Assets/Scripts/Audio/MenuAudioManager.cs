using UnityEngine;
using System.Collections.Generic;

public class MenuAudioManager : MonoBehaviour
{
    public static MenuAudioManager Instance { get; private set; }
    
    [Header("Button Sounds")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    
    [Header("Settings")]
    [SerializeField] private float volume = 0.5f;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        // Singleton
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // AudioSource erstellen
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = volume;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlayHoverSound()
    {
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound, volume * 0.6f); // Hover etwas leiser
        }
    }
    
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
}