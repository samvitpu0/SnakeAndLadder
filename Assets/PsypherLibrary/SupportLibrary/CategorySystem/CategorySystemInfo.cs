using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using PsypherLibrary.SupportLibrary.AudioManager;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels;
using PsypherLibrary.SupportLibrary.Utils.UI;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary.CategorySystem
{
    [Serializable]
    public class ContentType
    {
        public string Name;
        public Sprite Icon;

        [Header("Click Actions")]
        public bool MatchWithName;

        [SerializeField, Tooltip("This MonoBehaviour should always inherit ICategoryClickable")]
        public MonoBehaviour ActionOnClick;

        public ContentType()
        {
            Name = "icon name";
            Icon = null;
        }

        public ContentType(string name, Sprite icon)
        {
            Name = name;
            Icon = icon;
        }
    }

//category manager has to be always behind a splash scene [configs are loaded in the splash scene]
    public class CategorySystemInfo : MonoBehaviour
    {
        #region variable declaration

        [Header("Audio")]
        public AudioController AudioController;

        [Header("UI Panels")]
        [SerializeField]
        private UIViewController _viewController;

        public UIViewController ViewController
        {
            get
            {
                if (_viewController == null)
                {
                    _viewController = transform.root.GetComponent<UIViewController>();
                    if (_viewController == null)
                    {
                        _viewController = transform.root.gameObject.AddComponent<UIViewController>();
                    }
                }

                return _viewController;
            }
        }

        public CategoryRoot CategoryRoot;
        public UIType RootUiType = UIType.BaseView;
        public UIPanel CategoryContent;
        public UIPanel SubLevelContent;

        //Thumbnail Prefabs
        [Header("Prefabs")]
        [Tooltip("Only used when this system contains only items")]
        public GameObject ItemPrefab;

        [Tooltip("Contains both Item and category prefab")]
        public GameObject HybridPrefab;

        [Tooltip("Panel to instantiate when there are multiple levels")]
        public GameObject SubCategoryPanelPrefab;

        [Header("Data Related")]
        [Tooltip("Have to match field name as in App Config Json")]
        public string ConfigDataName;

        public bool AutoInit = true;
        public bool ContainsOnlyItems;
        public bool HasFavoriteCategory;
        public string FavIconLink = "http://www.freeiconspng.com/uploads/heart-favorite-icon-5.png";
        private CategoryCollection _categoryCollection;
        private JArray _categories;
        public Category CurrentParent;
        public bool ShowLoader = true;

        [Tooltip("Auto Activate the first level of the category UI, after its been populated")]
        public bool AutoActivateFirstContentUI = true;

        [Tooltip("Obeys the position tag in Json, else it will sort top to bottom")]
        public bool ItemPosFromJson = false;

        public List<ContentType> SupportedContentTypes = new List<ContentType>(); //content types this category system supports

        [Header("CallBack: called once after initialization")]
        public UnityEvent CallBackOnInitialize;

        [Header("Callback: called every time, a panel is populated")]
        public UnityEvent CallBackOnPopulate;

        [Header("Json Data")]
        public List<Category> AllCategories = new List<Category>();

        public List<Item> AllFreeItems = new List<Item>();
        public List<Item> AllPremiumItems = new List<Item>();

        public List<Item> AllItems = new List<Item>();

        //favorite item data
        public List<Item> FavoriteItems = new List<Item>();

        //debug-internal
        public string CurrentCategoryPath;

        #endregion

        #region Initialization

        void OnEnable()
        {
            if (SubLevelContent != null)
            {
                SubLevelContent.ContentHolder.Refresh();
            }
            else
            {
                CategoryContent.ContentHolder.Refresh();
            }
        }

        void Start()
        {
            if (AutoInit)
            {
                GetCategoryDetails();
            }
        }

        public void Initialize()
        {
            GetCategoryDetails();
        }

        void GetCategoryDetails()
        {
            _categoryCollection = LocalDataManager.Instance.GetConfig<CategoryCollection>(ConfigDataName);

            _categories = new JArray(_categoryCollection.Collections);
            //Debug.Log(_categories);
            AddDataToUI(_categories);
        }

        void AddDataToUI(JArray inCategories)
        {
            var jDataArray = new JArray(inCategories);
            if (CategoryRoot)
            {
                ViewController.Activate(CategoryRoot, RootUiType);
            }

            StartCoroutine(PopulateCategory(jDataArray, updateInBg: !AutoActivateFirstContentUI));
        }

        void InitFavoriteItems()
        {
            var items = new List<Item>();
            List<string> storedFavList;
            try
            {
                storedFavList = LocalDataManager.Instance.SaveData.FavoriteItems.TagItemList.Clone();

                if (storedFavList == null || !storedFavList.Any())
                {
                    Debug.Log("No Favorite items");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error: " + e);
                return;
            }

            //finding the fav videos
            foreach (var item in AllItems)
            {
                storedFavList.ForEach(favID =>
                {
                    if (item.UID == favID)
                    {
                        items.Add(item);
                        storedFavList.Remove(favID); //optimizing-> removing the fav that is already found
                    }
                });

                if (!storedFavList.Any()) //break the loop after finding all the favorites
                {
                    break;
                }
            }

            FavoriteItems = items;
        }

        #endregion

        #region Favorite Items

        public void AddFavoriteTag(Item iData)
        {
            if (!FavoriteItems.Exists(x => x.UID == iData.UID))
            {
                FavoriteItems.Add(iData);
                LocalDataManager.Instance.SaveData.FavoriteItems.AddItem(iData.UID);

                if (FavoriteItems.Count.Equals(1) && HasFavoriteCategory) //when first fav item is added
                {
                    var jDataArray = new JArray(_categories);
                    StartCoroutine(PopulateCategory(jDataArray, true));
                }
            }
        }

        public void RemoveFavoriteTag(Item iData)
        {
            Debug.Log("Removing");
            var itemToRemove = FavoriteItems.Find(x => x.UID == iData.UID);
            if (itemToRemove != null)
            {
                FavoriteItems.Remove(itemToRemove);
                LocalDataManager.Instance.SaveData.FavoriteItems.RemoveItem(iData.UID);
            }
        }

        #endregion

        #region Data

        public IEnumerator PopulateCategory(JArray inJData, bool updateInBg = false)
        {
            yield return new WaitForEndOfFrame();
            if (ShowLoader)
            {
                UILoader.Instance.StartLoader();
            }

            yield return new WaitForSeconds(0.3f);

            Debug.Log("populating: " + gameObject.name);

            if (inJData.Any())
            {
                var rootCategories = inJData.TryAndFindList<Category>(cat => cat.HasValues);
                var rootItems = inJData.TryAndFindList<Item>(item => item["Data"] != null);

                var allCatData = rootCategories.SelectMany(category => category.CategoryData).ToList();

                //trying to find all category types after the root level in hierarchy
                AllCategories = allCatData.TryAndFindList<Category>(i => i.HasValues);

                //adding the root categories into the _all categories list
                AllCategories = rootCategories.Concat(AllCategories).ToList();

                //getting all the items
                AllItems = AllCategories.SelectMany(category => category.CategoryData).TryAndFindList<Item>(item => item["Data"] != null).Concat(rootItems).ToList();

                yield return new WaitForEndOfFrame();

                AllItems = AllItems.OrderByDescending(item => item.UploadDate.FromUnixTime()).ToList();

                AllFreeItems.AddRange(from item in AllItems.Where(item => item.IsFree.Equals(true))
                    orderby item.UploadDate.FromUnixTime() descending
                    select item);

                AllPremiumItems.AddRange(from item in AllItems.Where(item => item.IsFree.Equals(false))
                    orderby item.UploadDate.FromUnixTime() descending
                    select item);

                yield return new WaitForEndOfFrame();

                Debug.Log("Data is set");
                InitFavoriteItems();
                CallBackOnInitialize.Invoke();

                //if we want to show the favorite as category and if there is favorite items available
                if (HasFavoriteCategory && FavoriteItems.Any())
                {
                    var favCat = new Category("Favorites", FavIconLink, null); //todo:later change the icon image from the mainJson
                    var favToken = JToken.FromObject(favCat);
                    inJData.AddFirst(favToken);
                }

                yield return StartCoroutine(PopulateContents(inJData, false, createNewPanel: false, updateInBg: updateInBg));
            }

            yield return new WaitForEndOfFrame();

            if (ShowLoader)
            {
                UILoader.Instance.StopLoader(true);
            }
        }

        //call to 
        public IEnumerator PopulateContents(JArray inJData, bool subLevel = true, string parentName = "", bool createNewPanel = true, Category parentData = null, ThumbnailHelper parentThumbnail = null, bool updateInBg = false, bool forceBase = false, bool forceNumberThumbnails = false)
        {
            //Debug.Log(inJData);
            if (inJData == null)
            {
                //if there is no data, there is a possibility, it can be a custom category
                //add custom on click
                if (parentData != null)
                {
                    OnClick(JToken.FromObject(parentData), parentThumbnail);
                }

                yield break;
            }

            #region Organising @inJData

            //organising the category and item data for proper sorting when displayed
            var jList = inJData.ToList();
            var allCategories = JArray.FromObject(jList.TryAndFindList<Category>(cat => cat.HasValues));
            var allItems = jList.TryAndFindList<Item>(item => item["Data"] != null);

            /*//sorting the item list -> newest always at top
        allItems.Sort((a, b) => b.UploadDate.FromUnixTime().CompareTo(a.UploadDate.FromUnixTime())); //descending order*/

            if (ItemPosFromJson)
            {
                allItems = (from item in allItems
                    orderby item.PositionInList
                    select item).ToList();
            }

            var freeItems = allItems.FindAll(x => x.IsFree);
            var premiumItems = allItems.FindAll(x => !x.IsFree);
            var organisedItems = JArray.FromObject(freeItems.Concat(premiumItems));
            var organisedData = allCategories.Concat(organisedItems).ToList();

            #endregion

            CurrentParent = parentData;

            if (createNewPanel)
                CreateSubPanel(parentData != null ? parentData.Name : parentName, parentData);

            var holderPanel = subLevel ? SubLevelContent : CategoryContent;

            yield return new WaitForEndOfFrame();

            if (holderPanel == null) yield break; //if no panel is there return

            if (!updateInBg) //activate the panel if this is false
            {
                ViewController.Activate(holderPanel, UIType.NormalView, () =>
                {
                    if (ShowLoader)
                    {
                        UILoader.Instance.StartLoader();
                    }
                });
            }

            var thumbnailPrefab = HybridPrefab;
            if (ContainsOnlyItems)
                thumbnailPrefab = ItemPrefab;

            //getting the override script
            var overrideRule = GetComponent<CategoryPopulateOverride>();

            //applying override populate script
            if (overrideRule != null && !forceBase)
            {
                overrideRule.OverrideCreateAndFill(holderPanel, thumbnailPrefab, organisedData, ContainsOnlyItems, forceNumberThumbnails);
            }
            else
            {
                CreateAndFill(holderPanel, thumbnailPrefab, organisedData, ContainsOnlyItems, forceNumberThumbnails);
            }

            CallBackOnPopulate.Invoke();

            yield return new WaitForSeconds(0.5f);
            if (ShowLoader)
            {
                UILoader.Instance.StopLoader(true);
            }
        }

        void CreateAndFill(UIPanel panelContainer, GameObject thumbnailPrefab, List<JToken> jData, bool containsOnlyItems, bool forceNumberThumbnails)
        {
            panelContainer.ContentHolder.SetPrefab(thumbnailPrefab).SetData(jData).SetFunction((data, index, obj) =>
            {
                var cData = (List<JToken>) data;
                var tHybrid = obj.GetComponent<ThumbnailHybrid>();

                var catObj = cData[index].TryAndFind<Category>(cat => cat.HasValues);
                var itemObj = cData[index].TryAndFind<Item>(item => item["Data"] != null);

                if (catObj != null)
                {
                    var tHelper = tHybrid.SetThumbnailType(ThumbnailTypes.Category);
                    tHelper.SetContentType(ThumbnailTypes.Category);
                    tHelper.SetDetails(catObj.Name, catObj.Thumbnail, null, () => StartCoroutine(PopulateContents(catObj.CategoryData, parentName: catObj.Name, parentData: catObj, parentThumbnail: tHelper)), optionalData: new Dictionary<string, object>()
                    {
                        {"extraText", (index + 1)}
                    });
                }

                if (itemObj != null)
                {
                    //if data only contains items, no need for hybrid
                    var tHelper = containsOnlyItems ? obj.GetComponent<ThumbnailHelper>() : tHybrid.SetThumbnailType(ThumbnailTypes.Item);

                    tHelper.SetContentType(ThumbnailTypes.Item, itemObj.ContentType);
                    tHelper.SetDetails(itemObj.Name, itemObj.Thumbnail, () => OnInitialize(itemObj, tHelper), () => OnClick(cData[index], tHelper), optionalData: new Dictionary<string, object>()
                    {
                        {"extraText", (index + 1)}
                    });
                }
            }).Initialize();
        }

        #endregion

        #region actions

        public void CreateSubPanel(String panelName, Category parentData = null)
        {
            var panel = Instantiate(SubCategoryPanelPrefab, CategoryRoot.transform, false);
            panel.Activate();
            var panelUI = panel.GetComponent<UIPanel>();
            if (panelUI)
            {
                SubLevelContent = panelUI;
                panelUI.transform.position = Vector3.zero;
                panelUI.PanelTitle.SetText(panelName);

                //clearing the callback actions
                SubLevelContent.CallBackOnActivate = null;
                SubLevelContent.CallBackOnDeactivate = null;
                SubLevelContent.CallBackOnReadyToDeactivate = null;
                SubLevelContent.CallBackOnReadyToDeactivate = null;

                //adding new callback actions
                SubLevelContent.CallBackOnReadyToDeactivate += () =>
                {
                    //todo: these are called every time a sub-panel is deactivated, later we should reduce the number of time these are called. ATM Not much noticeable performance cost though.
                    LocalDataManager.Instance.Save();
                    InitFavoriteItems();
                    CategoryContent.ContentHolder.Refresh();
                };
            }
        }

        public void OnInitialize(Item item, ThumbnailHelper thumbnail)
        {
            // using regex to parse every http url and check if they are all download to show if the item is downloaded

            var linkParser = new Regex(@"\b(?:https?://|www\.)\S+(\.mp4|.mp3|.png)\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var parsedLinks = linkParser.Matches(item.Data.ToString()).Cast<Match>().Select(x => x.Value).ToList();

            var isFavorite = FavoriteItems.Any(x => x.UID.Equals(item.UID));
            //Debug.Log("Item: " + item.UID + ", downloaded: " + FileManager.IsFileDownloaded(parsedLinks));
            /*foreach (var parsedLink in parsedLinks)
        {
            Debug.Log(parsedLink);
        }*/

            if (parsedLinks.Any())
            {
                thumbnail.SetThumbnailState(FileManager.IsFileDownloaded(parsedLinks), true, isFavorite, isOn =>
                {
                    if (isOn)
                    {
                        AddFavoriteTag(item);
                    }
                    else
                    {
                        RemoveFavoriteTag(item);
                    }
                });

                if (FileManager.IsDownloadInitiated(parsedLinks.ToArray()))
                {
                    thumbnail.IsDownloading = true;

                    var imageUrls = parsedLinks.Where(x => x.Contains(".png")).ToArray();
                    var videoUrls = parsedLinks.Where(x => x.Contains(".mp4")).ToArray();
                    var soundUrls = parsedLinks.Where(x => x.Contains(".mp3")).ToArray();

                    var downloadFactor = 0;

                    if (imageUrls.Any())
                    {
                        downloadFactor++;

                        FileManager.Instance.GetImage(urls: imageUrls, onComplete: (downloadedUrls) => thumbnail.SetThumbnailState(true), owner: this, onFail: () =>
                        {
                            Debug.Log("file Retrival error!!");
                            UIToastNotification.Instance.TriggerToast(LocalizedText.GetLocalizedText("OnDownloadInterrupt") ?? "Oops Download failed, please try again.", 2f);
                            thumbnail.SetThumbnailState(false);
                        }, onProgress: (progress) => thumbnail.ShowProgress(progress / downloadFactor), needDataOnComplete: false);
                    }

                    if (videoUrls.Any())
                    {
                        downloadFactor++;

                        FileManager.Instance.GetVideo(urls: videoUrls, onComplete: (downloadedUrls) => thumbnail.SetThumbnailState(true), owner: this, onFail: () =>
                        {
                            Debug.Log("file Retrival error!!");
                            UIToastNotification.Instance.TriggerToast(LocalizedText.GetLocalizedText("OnDownloadInterrupt") ?? "Oops Download failed, please try again.", 2f);

                            thumbnail.SetThumbnailState(false);
                        }, onProgress: (progress) => thumbnail.ShowProgress(progress / downloadFactor), needDataOnComplete: false);
                    }

                    if (soundUrls.Any())
                    {
                        downloadFactor++;

                        FileManager.Instance.GetVideo(urls: soundUrls, onComplete: (downloadedUrls) => thumbnail.SetThumbnailState(true), owner: this, onFail: () =>
                        {
                            Debug.Log("file Retrival error!!");
                            UIToastNotification.Instance.TriggerToast(LocalizedText.GetLocalizedText("OnDownloadInterrupt") ?? "Oops Download failed, please try again.", 2f);
                            thumbnail.SetThumbnailState(false);
                        }, onProgress: (progress) => thumbnail.ShowProgress(progress / downloadFactor), needDataOnComplete: false);
                    }

                    //thumbnail.SetThumbnailState(downloadComplete);
                }
            }

            thumbnail.SetRelatedData(item);

            if (!item.IsFree && !LocalDataManager.Instance.SaveData.GetPremiumStatus())
            {
                thumbnail.SetFreeContent(false);
            }
            else
            {
                thumbnail.SetFreeContent(true);
            }
        }

        public void OnClick(JToken jData, ThumbnailHelper thumbnail)
        {
            //todo: determine the item type send to respective scene [content type identifier] and show the content[hook to systems other developers have created]
            var catObj = jData.TryAndFind<Category>(x => x.HasValues);
            var item = jData.TryAndFind<Item>(x => x["Data"] != null);

            thumbnail.IsDownloading = false; //need to set to true onClick implementation

            foreach (var supportedContentType in SupportedContentTypes)
            {
                if (supportedContentType.ActionOnClick == null) continue;

                ICategoryClickable click = null;

                //for custom category click [very few use case]
                if (catObj != null)
                {
                    if (supportedContentType.Name.Equals(catObj.Name))
                    {
                        click = thumbnail.gameObject.GetComponent(supportedContentType.ActionOnClick.GetType()) as ICategoryClickable ?? thumbnail.gameObject.AddComponent(supportedContentType.ActionOnClick.GetType()) as ICategoryClickable;
                    }
                }

                //for item click [primary use]
                if (item != null)
                {
                    if (supportedContentType.MatchWithName)
                    {
                        if (supportedContentType.Name.Equals(item.Name))
                        {
                            click = thumbnail.gameObject.GetComponent(supportedContentType.ActionOnClick.GetType()) as ICategoryClickable ?? thumbnail.gameObject.AddComponent(supportedContentType.ActionOnClick.GetType()) as ICategoryClickable;
                        }
                    }
                    else
                    {
                        if (supportedContentType.Name.Equals(item.ContentType))
                        {
                            click = thumbnail.gameObject.GetComponent(supportedContentType.ActionOnClick.GetType()) as ICategoryClickable ?? thumbnail.gameObject.AddComponent(supportedContentType.ActionOnClick.GetType()) as ICategoryClickable;
                        }
                    }
                }

                if (click != null)
                {
                    click.OnClick(this, thumbnail);
                }
            }
        }

        public void RefreshCategorySystem()
        {
            //refresh the fav items by calling init function
            InitFavoriteItems();
            //and then have to call populate so that the fav is added to the category system - this has to be done cause, if fav tag is added from the story book page, category system has no way to know about it.
            if (FavoriteItems.Count.Equals(1) && HasFavoriteCategory) //when first fav item is added
            {
                var jDataArray = new JArray(_categories);
                StartCoroutine(PopulateCategory(jDataArray, true));
            }

            //root
            if (CategoryContent)
            {
                CategoryContent.ContentHolder.Refresh();
            }

            //sublevel
            if (SubLevelContent)
            {
                SubLevelContent.ContentHolder.Refresh();
            }
        }

        #endregion
    }
}
#endif