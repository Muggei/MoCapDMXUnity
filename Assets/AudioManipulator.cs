using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManipulator : MonoBehaviour {

    [InspectorButton("StartAudio")]
    public bool StartClip;

    public AudioMixerGroup mastermixer;
    public AudioClip audioclip;
    private AudioSource audioSource;

    void Start() {
        audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = mastermixer;
        audioSource.clip = audioclip;

    }

    public void StartAudio() {
        audioSource.Play();
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
