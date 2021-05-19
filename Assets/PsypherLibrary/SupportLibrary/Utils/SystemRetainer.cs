using System;
using System.Collections.Generic;
using System.Linq;
using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PsypherLibrary.SupportLibrary.Utils
{
    public class SystemRetainer : GenericDontDestroyOnLoad<SystemRetainer>
    {
        public Action OnActivated;
        public UnityEvent OnActivatedEvent;

        public List<EScenes> ActivateInScenes;

        private Canvas _canvas;

        protected override void OnLevelLoaded(Scene scene)
        {
            base.OnLevelLoaded(scene);

            _canvas = GetComponent<Canvas>();

            if (_canvas)
            {
                _canvas.worldCamera = Camera.main;
            }

            if (ActivateInScenes != null && ActivateInScenes.Any() && ActivateInScenes.Contains(x => x.GetDescription().Equals(scene.name)))
            {
                gameObject.Activate();

                //refresh contents
                OnActivated.SafeInvoke();
                OnActivatedEvent.SafeInvoke();
            }
            else
            {
                gameObject.Deactivate();
            }
        }
    }
}