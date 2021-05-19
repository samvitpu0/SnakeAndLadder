using System;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    public class TransformChangedEvent : MonoBehaviour
    {
        [Serializable]
        public enum ExtremeIndex
        {
            FirstSibling,
            LastSibling
        }

        public UnityEvent OnTransformChanged;
        public bool EnableEvent;

        public int SiblingIndex;
        public bool UseExtremeIndex;
        public ExtremeIndex ExtremeSibling;

        private int _oldChildCount;

        // Use this for initialization
        void Awake()
        {
            _oldChildCount = transform.parent.childCount;
        }

        void Update()
        {
            if (transform.parent.childCount > _oldChildCount)
            {
                SetSiblingIndex();
            }

            if (transform.hasChanged)
            {
                if (EnableEvent)
                    OnTransformChanged.SafeInvoke();
            }
        }

        void SetSiblingIndex()
        {
            if (UseExtremeIndex)
            {
                switch (ExtremeSibling)
                {
                    case ExtremeIndex.FirstSibling:
                    {
                        transform.SetAsFirstSibling();
                    }
                        break;
                    case ExtremeIndex.LastSibling:
                    {
                        transform.SetAsLastSibling();
                    }
                        break;
                }
            }
            else
            {
                transform.SetSiblingIndex(SiblingIndex);
            }
        }
    }
}