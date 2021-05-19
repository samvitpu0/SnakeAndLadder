using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary.CategorySystem
{
    public class CategoryRoot : UIPanel
    {
        #region variable declaration

        private CategorySystemInfo _categorySystemInfo;

        public CategorySystemInfo CategorySystemInfo
        {
            get
            {
                if (_categorySystemInfo == null)
                {
                    var root = transform.root;
                    _categorySystemInfo = root.GetComponent<CategorySystemInfo>() ?? root.GetComponentInChildren<CategorySystemInfo>();
                }

                return _categorySystemInfo;
            }
        }

        [Header("Additional Panels")]
        public UIPanel SearchPanel;


        [Header("Extra Navigation")]
        public ToggleGroup NavGroup;

        public Toggle NewButton;
        public Toggle FavButton;
        public Toggle FreeButton;

        public Button SearchButton;

        #endregion

        #region Initialization

        protected override void OnEnable()
        {
            base.OnEnable();
            CategorySystemInfo.CategoryContent.CallBackOnReadyToActivate += () =>
            {
                if (NavGroup)
                    NavGroup.SetAllTogglesOff();
            };
        }


        void Start()
        {
            if (NewButton)
            {
                NewButton.onValueChanged.RemoveAllListeners();
                NewButton.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                        StartCoroutine(OnPressNew());
                });
            }

            if (FreeButton)
            {
                FreeButton.onValueChanged.RemoveAllListeners();
                FreeButton.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                        StartCoroutine(OnPressFree());
                });
            }

            if (FavButton)
            {
                FavButton.onValueChanged.RemoveAllListeners();
                FavButton.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                        StartCoroutine(OnPressFav());
                });
            }

            if (SearchButton)
            {
                SearchButton.onClick.RemoveAllListeners();
                SearchButton.onClick.AddListener(PressSearch);
            }
        }

        #endregion

        #region data

        #endregion

        #region button actions

        IEnumerator OnPressNew()
        {
            UILoader.Instance.StartLoader();

            List<Item> newItems = new List<Item>();
            var today = DateTimeExtensions.GetCurrentUnixTime().ToString();

            if (CategorySystemInfo.AllItems.Any())
            {
                foreach (var item in CategorySystemInfo.AllItems)
                {
                    //new Contents are now considered only if they are a t most 7 days old...more than that, it will ignore
                    if (DateTimeExtensions.DifferenceBetweenTwoUnixTime(today, item.UploadDate) <
                        new TimeSpan(7, 0, 0, 0).TotalSeconds)
                    {
                        newItems.Add(item);
                    }

                    yield return true;
                }

                yield return StartCoroutine(CategorySystemInfo.PopulateContents(JArray.FromObject(newItems), parentName: "New Arrivals"));
            }

            UILoader.Instance.StopLoader(true);
            yield return new WaitForEndOfFrame();
        }

        IEnumerator OnPressFree()
        {
            yield return StartCoroutine(CategorySystemInfo.PopulateContents(JArray.FromObject(CategorySystemInfo.AllFreeItems), parentName: "Free for You"));

            yield return new WaitForEndOfFrame();
        }

        IEnumerator OnPressFav()
        {
            UILoader.Instance.StartLoader();

            yield return StartCoroutine(CategorySystemInfo.PopulateContents(JArray.FromObject(CategorySystemInfo.FavoriteItems),
                parentData: new Category("Your Favorites", CategorySystemInfo.FavIconLink, null)));

            yield return new WaitForEndOfFrame();

            UILoader.Instance.StopLoader(true);
        }

        IEnumerator OnPressSearch()
        {
            SearchManager.SetData(CategorySystemInfo.AllItems);
            yield return new WaitForEndOfFrame();

            if (SearchPanel)
            {
                ViewController.Activate(SearchPanel);
            }
        }

        //---can be mapped to buttons
        public void PressFav()
        {
            StartCoroutine(OnPressFav());
        }

        public void PressNew()
        {
            StartCoroutine(OnPressNew());
        }

        public void PressFree()
        {
            StartCoroutine(OnPressFree());
        }

        public void PressSearch()
        {
            StartCoroutine(OnPressSearch());
        }

        #endregion
    }
}

#endif