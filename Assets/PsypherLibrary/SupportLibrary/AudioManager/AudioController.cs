using System;
using System.Collections.Generic;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioController : MonoBehaviour
    {
        [Header("Original Script")]
        public List<AudioComponent> AudioFiles;

        private bool _IsInitialized = false;

        public bool IsInitialized
        {
            get { return _IsInitialized; }
        }

        public static float Volume = 1;
        public static float AudioFadeInDelay = 0;
        public static float AudioFadeOutDelay = 0;
        readonly Dictionary<string, AudioSource> AudioEvents = new Dictionary<string, AudioSource>();

        [HideInInspector]
        AudioSource _audioSource;

        public Action OnControllerInit;

        public AudioSource GetAudioSource
        {
            get
            {
                if (_audioSource == null)
                    _audioSource = GetComponent<AudioSource>();
                return _audioSource;
            }
        }

        public static Dictionary<string, AudioClip> AudioClips = new Dictionary<string, AudioClip>(); //Tag,Clip
        public static bool LocalizedAudioCached = false;

        static readonly Dictionary<EventTriggerType, Type> ListenerTypes = new Dictionary<EventTriggerType, Type>()
        {
            {EventTriggerType.PointerEnter, typeof(AudioOnPointerEnter)},
            {EventTriggerType.PointerExit, typeof(AudioOnPointerExit)},
            {EventTriggerType.PointerDown, typeof(AudioOnPointerDown)},
            {EventTriggerType.PointerUp, typeof(AudioOnPointerUp)},
            {EventTriggerType.PointerClick, typeof(AudioOnPointerClick)},
            {EventTriggerType.Drag, typeof(AudioOnDrag)},
            {EventTriggerType.Drop, typeof(AudioOnDrop)},
            {EventTriggerType.Scroll, typeof(AudioOnScroll)},
            {EventTriggerType.UpdateSelected, typeof(AudioOnUpdateSelected)},
            {EventTriggerType.Select, typeof(AudioOnSelect)},
            {EventTriggerType.Deselect, typeof(AudioOnDeselect)},
            {EventTriggerType.Move, typeof(AudioOnMove)},
            {EventTriggerType.EndDrag, typeof(AudioOnEndDrag)},
            {EventTriggerType.BeginDrag, typeof(AudioOnBeginDrag)},
            {EventTriggerType.Submit, typeof(AudioOnSubmit)},
            {EventTriggerType.Cancel, typeof(AudioOnCancel)},
        };

        public enum EAudioLayer
        {
            Sound,
            Music
        }

        void OnEnable()
        {
            ConfigManager.OnAudioCacheSuccess += InitializeAudioEffects;
        }

        void OnDisable()
        {
            ConfigManager.OnAudioCacheSuccess -= InitializeAudioEffects;
        }

        void Awake()
        {
            InitializeAudioEffects();
        }

        void InitializeAudioEffects()
        {
            _IsInitialized = false;
            AudioEvents.Clear();
            gameObject.DestroyChildren();
            foreach (var audFile in AudioFiles)
            {
                foreach (var item in audFile.GameObjects)
                {
                    if (item != null)
                        Destroy(item.GetComponent(ListenerTypes[audFile.Event]));
                }
            }

            foreach (var audFile in AudioFiles)
            {
                if (LocalDataManager.Instance.SaveData.AudioLayerToggle.ContainsKey(audFile.AudioLayer) && LocalDataManager.Instance.SaveData.AudioLayerToggle[audFile.AudioLayer] == false)
                    continue;
                if (audFile.CustomTriggerTag != string.Empty)
                {
                    AudioSource aSource = GetAudioSource;
                    if (audFile.NeedSource)
                    {
                        aSource = new GameObject().AddComponent<AudioSource>();
                        aSource.gameObject.transform.SetParent(transform);
                    }

                    aSource.clip = !string.Equals(LocalDataManager.Instance.SaveData.CurrentLanguage, BaseConstants.DEFAULT_LANGUAGE) && AudioClips.ContainsKey(audFile.CustomTriggerTag) ? AudioClips[audFile.CustomTriggerTag] : audFile.ClipToPlay;
                    aSource.loop = audFile.NeedSource && audFile.DoLoop;
                    aSource.playOnAwake = audFile.PlayOnAwake;
                    aSource.volume = audFile.Volume;

                    AudioEvents.SafeAdd(audFile.CustomTriggerTag, aSource);

                    if (audFile.PlayOnAwake)
                        TriggerAudio(audFile.CustomTriggerTag);
                }
                else
                {
                    foreach (var item in audFile.GameObjects)
                    {
                        if (item == null)
                            continue;
                        AudioSource aSource = GetAudioSource;
                        if (audFile.NeedSource)
                            aSource = item.AddComponent<AudioSource>();

                        var aBase = item.AddComponent(ListenerTypes[audFile.Event]) as AudioBase;
                        aBase.SetAudioClip(audFile.ClipToPlay);
                        aSource.loop = audFile.NeedSource && audFile.DoLoop;
                        aSource.playOnAwake = audFile.PlayOnAwake;

                        if (audFile.PlayOnAwake)
                            aBase.PlayAudio();
                    }
                }
            }

            _IsInitialized = true;
            OnControllerInit.SafeInvoke();
        }

        public void TriggerAudio(string audioTag, Action onComplete = null)
        {
            if (AudioEvents.ContainsKey(audioTag))
            {
                if (AudioEvents[audioTag].clip == null)
                {
                    Debug.Log("Audio clip for [" + audioTag + "] not Linked in Audio Manager");
                    return;
                }

                AudioEvents[audioTag].Play();
                DOTween.Sequence().AppendInterval(AudioEvents[audioTag].clip.length).AppendCallback(onComplete.SafeInvoke).Play();
            }

            //else
            //    Debug.Log(string.Format("Audio Trigger Tag Not Found : {0}", tag));
        }

        public void StopAudio(string tag, Action onComplete = null)
        {
            if (AudioEvents.ContainsKey(tag))
            {
                AudioEvents[tag].Stop();
                DOTween.Sequence().AppendInterval(AudioEvents[tag].clip.length).AppendCallback(onComplete.SafeInvoke).Play();
            }

            //else
            //    Debug.Log(string.Format("Audio Trigger Tag Not Found : {0}", tag));
        }

        public void TriggerAudio(string audioTag)
        {
            TriggerAudio(audioTag, null);
        }

        public void StopAudio(string audioTag)
        {
            StopAudio(audioTag, null);
        }

        [ContextMenu("DiscardNullObjects")]
        public void DiscardNullObjects()
        {
            foreach (AudioComponent t in AudioFiles)
            {
                t.GameObjects.RemoveAll(y => y == null);
            }
        }
    }
}