using System;
using System.Collections.Generic;
using System.Linq;
using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.BrandingSystem
{
    public class BrandingManager : GenericManager<BrandingManager>
    {
        #region Supports Types

        [Serializable]
        public enum BrandingType
        {
            Image,
            Video
        }

        [Serializable]
        public class BrandingData : CoreData
        {
            [Header("BrandingData")]
            public BrandingType Type;

            public float TransitionTime = 0.5f;
            public float AnimationTime = 1f;
            public float DisplayTime = 1f;

            [Tooltip("For Image Ads, don't try to add both types into one Branding Data")]
            public Image ImageAdDisplay;

            [Tooltip("For Video Ads, don't try to add both types into one Branding Data")]
            public VideoPlayer VideoAdDisplay;

            [Tooltip("Ads Images, in-case of Image type")]
            public List<Sprite> AdsImages;

            [Tooltip("Ads Videos, in-case of Video type")]
            public List<VideoClip> AdsVideos;
        }

        #endregion

        #region Feilds and Properties

        private bool _isInitialized = false;

        public bool AutoInitialize = false;

        public List<BrandAdElement> BrandAdElements = new List<BrandAdElement>();

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        #endregion

        #region Initialization

        protected override void OnLevelLoaded(int levelIndex)
        {
            base.OnLevelLoaded(levelIndex);

            BrandAdElements.Clear();
            BrandAdElements = ComponentExtensions.FindObjectsOfTypeIncludingInactive<BrandAdElement>();

            if (AutoInitialize)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (BrandAdElements != null && BrandAdElements.Any())
            {
                //initialize all referenced Brand elements
                BrandAdElements.ForEach(x => x.Initialize());

                _isInitialized = true;
            }
        }

        #endregion

        #region Actions

        public void Play(string brandElementUid)
        {
            if (!_isInitialized) return;
            GetBrandElementById(brandElementUid).Play();
        }

        public void Play(BrandAdElement brandElement)
        {
            if (!_isInitialized) return;
            brandElement.Play();
        }

        public int PlayAll()
        {
            if (!_isInitialized) return 0;

            int totalAds = 0;

            BrandAdElements.ForEach(x =>
            {
                x.Play();
                totalAds++;
            });

            return totalAds;
        }

        public void Stop(string brandElementUid)
        {
            if (!_isInitialized) return;
            GetBrandElementById(brandElementUid).Stop();
        }

        public void Stop(BrandAdElement brandElement)
        {
            if (!_isInitialized) return;
            brandElement.Stop();
        }

        public int StopAll()
        {
            if (!_isInitialized) return 0;

            int totalAds = 0;

            BrandAdElements.ForEach(x =>
            {
                x.Stop();
                totalAds++;
            });

            return totalAds;
        }

        public void Restart(string brandElementUid)
        {
            if (!_isInitialized) return;
            GetBrandElementById(brandElementUid).Restart();
        }

        public void Restart(BrandAdElement brandElement)
        {
            if (!_isInitialized) return;
            brandElement.Restart();
        }

        public int RestartAll()
        {
            int totalAds = 0;

            if (!_isInitialized) return 0;

            BrandAdElements.ForEach(x =>
            {
                x.Restart();
                totalAds++;
            });

            return totalAds;
        }

        #endregion

        #region Utilities

        private BrandAdElement GetBrandElementById(string id)
        {
            return BrandAdElements.Find(x => x.UID.Equals(id));
        }

        #endregion
    }
}