using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioSource musicA;
    [SerializeField] AudioSource musicB;

    private void Awake()
    {
        CheckpointController.Instance.OnLapStart += Instance_OnLapStart;
        musicB.mute = true;
    }

    private void Instance_OnLapStart()
    {
        musicB.mute = false;
    }
}
