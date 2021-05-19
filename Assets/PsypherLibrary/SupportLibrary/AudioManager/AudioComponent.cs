using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager
{
    [Serializable]
    public class AudioComponent
    {
        public bool NeedSource = true;
        public bool PlayOnAwake = true;
        public bool DoLoop = true;
        public string CustomTriggerTag = string.Empty;
        public AudioController.EAudioLayer AudioLayer;
        public AudioClip ClipToPlay;
        public EventTriggerType Event;

        [Range(0, 1)]
        public float Volume = 1;

        public List<GameObject> GameObjects = new List<GameObject>();
    }
}