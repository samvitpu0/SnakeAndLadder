using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.AudioManager
{
    [RequireComponent(typeof(Selectable))]
    public class AudioBase : MonoBehaviour
    {
        AudioClip audioClip;
        AudioSource _audioSource;

        public AudioSource GetAudioSource
        {
            get
            {
                if (_audioSource == null)
                    _audioSource = GetComponent<AudioSource>();
                return _audioSource;
            }
        }

        public void PlayAudio()
        {
            GetAudioSource.clip = GetAudioClip();
            // Debug.Log("Audio Clip Name : " + GetAudioClip().name + " , Played From : " + gameObject.name);
            GetAudioSource.volume = AudioController.Volume;
            GetAudioSource.Play();
        }

        public void FadeInAudio()
        {
            StartCoroutine(FadeInAudioCoroutine());
        }

        public IEnumerator FadeInAudioCoroutine()
        {
            if (!GetAudioSource.isPlaying)
            {
                GetAudioSource.volume = 0;
                GetAudioSource.Play();
            }

            yield return GetAudioSource.DOFade(AudioController.Volume, AudioController.AudioFadeInDelay).WaitForCompletion();
        }

        public void SetAudioVolume(float Volume0to1)
        {
            AudioController.Volume = Volume0to1;
            GetAudioSource.volume = Volume0to1;
        }

        public void StopAudio()
        {
            GetAudioSource.volume = 0;
            GetAudioSource.Stop();
        }

        public void FadeOutAudio()
        {
            StartCoroutine(FadeOutAudioCoroutine());
        }

        public IEnumerator FadeOutAudioCoroutine()
        {
            yield return GetAudioSource.DOFade(0, AudioController.AudioFadeOutDelay).WaitForCompletion();
        }

        public void PauseAudio()
        {
            GetAudioSource.Pause();
        }

        public void ResumeAudio()
        {
            GetAudioSource.UnPause();
        }

        public void SetAudioClip(AudioClip audClip)
        {
            audioClip = audClip;
        }

        public AudioClip GetAudioClip()
        {
            return audioClip;
        }
    }
}