using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManipulator : MonoBehaviour {


    public AudioMixerGroup mastermixer;
    public AudioClip[] audioclip;
    public int SelectAudioClip = 0;
    private AudioSource audioSource;


    [InspectorButton("StartAudio")]
    public bool StartClip;
    [InspectorButton("PauseAudio")]
    public bool PauseClip;
    [InspectorButton("StopAudio")]
    public bool StopClip;

    void Start() {
        audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = mastermixer;
        
    }

    public void StartAudio() {
        audioSource.clip = audioclip[SelectAudioClip];
        audioSource.Play();
    }

    public void PauseAudio() {
        audioSource.Pause();
    }

    public void StartOrPauseAudio()
    {
        if (audioSource.isPlaying)
        {
            this.PauseAudio();
        }
        else {
            this.StartAudio();
        }

    }

    public void StopAudio() {
        audioSource.Stop();
    }

    public void SetMasterVolume(float volume) {
        mastermixer.audioMixer.SetFloat("masterVol", volume);
    }

    public void SetHighPassFilter(float frequency) {
        mastermixer.audioMixer.SetFloat("highPassFreq", frequency);
    }
}
