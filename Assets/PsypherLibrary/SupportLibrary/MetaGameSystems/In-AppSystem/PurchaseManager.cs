using System;
using System.Collections.Generic;
using System.Linq;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using UnityEngine;

#if ENABLE_PURCHASE
namespace PsypherLibrary.SupportLibrary.MetaGameSystems
{
    public class PurchaseManager : GenericManager<PurchaseManager>, IStoreListener
    {
        private static IStoreController StoreController = null;
        private static IExtensionProvider StoreExtensionProvider = null;

        public static Action<Product> OnPurchaseSuccess;
        public static Action OnInitSuccess;
        public static Action OnInitFailure;
        public static Action<Product, PurchaseFailureReason> OnPurchaseFailure;

        private static Product PremiumProduct = null;
        private static IPurchaseReceipt PremiumProductReceipt = null;

        public bool FailInEditor = false;
        private bool _isInitializing = false;
        private IAppleConfiguration appleConfig;
        private bool _isRestoreAllowed = false;

        //complete iap catalog
        private ProductCatalog _productCatalog;

        public ProductCatalog Catalog
        {
            get { return _productCatalog; }
        }

        public bool IsInitializing
        {
            get { return _isInitializing; }
        }

        public void InitializeStore()
        {
            Debug.Log("Initializing Store");

            var catalogJson = LocalDataManager.Instance.GetConfigJson(typeof(ProductCatalog).Name);
            _productCatalog = ProductCatalog.Deserialize(catalogJson);

            if (IsInitialized())
            {
                OnInitSuccess.SafeInvoke();
                return;
            }

            if (_isInitializing)
            {
                return;
            }

            _isInitializing = true;
            var module = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(module);

/*        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        _productCatalog = JsonConvert.DeserializeObject<ProductCatalog>(catalogJson, settings);*/

#if UNITY_IOS
        appleConfig = builder.Configure<IAppleConfiguration> ();
#endif
            foreach (var product in _productCatalog.allProducts)
            {
                Debug.Log("Product: " + product.id + ", type: " + product.type);
                if (product.allStoreIDs.Count > 0)
                {
                    var ids = new IDs();
                    foreach (var storeId in product.allStoreIDs)
                    {
                        ids.Add(storeId.id, storeId.store);
                    }

                    builder.AddProduct(product.id, product.type, ids);
                }
                else
                {
                    builder.AddProduct(product.id, product.type);
                }
            }

            UnityPurchasing.Initialize(this, builder);
        }

        void RevalidateSubscription()
        {
            Debug.Log("Re-validating After Initialization");
            try
            {
#if UNITY_ANDROID

                var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                    null, Application.identifier);

                _isRestoreAllowed = false;
                LocalDataManager.Instance.SaveData.NonConsumableBuy.Clear();

                foreach (var product in StoreController.products.all.ToList())
                {
                    var result = validator.Validate(product.receipt);

                    if (product.hasReceipt)
                    {
                        _isRestoreAllowed = true;

                        switch (product.definition.type)
                        {
                            case ProductType.Consumable:
                                //no need to store consumable values
                                break;
                            case ProductType.NonConsumable:
                            {
                                LocalDataManager.Instance.SaveData.NonConsumableBuy.AddUnique(product.metadata.localizedTitle);
                            }
                                break;
                            case ProductType.Subscription:
                            {
                                /*LocalDataManager.Instance.SaveData.SubscriptionDetails.SafeAdd("Title", product.metadata.localizedTitle);
                    LocalDataManager.Instance.SaveData.SubscriptionDetails.SafeAdd("Date", result[0].purchaseDate.ToShortDateString());*/
                            }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                //todo: refactor as above for ios
#elif UNITY_IOS
             Debug.Log("Revalidating After iOS receipt");
            var validator = new CrossPlatformValidator(null,
                AppleTangle.Data(), Application.identifier);
            string appReceipt = appleConfig.appReceipt;
            string unityIAPReceipt = "{ \"Store\": \"AppleAppStore\", \"Payload\": \"" + appReceipt + "\" }";
            var result = validator.Validate(unityIAPReceipt);
    
            _isRestoreAllowed = false;
            LocalDataManager.Instance.SaveData.NonConsumableBuy.Clear();
            Debug.Log("  LocalDataManager.Instance.SaveData.NonConsumableBuy Cleared");

            foreach (IPurchaseReceipt productReceipt in result)
            {
                var prodReceipt = productReceipt as AppleInAppPurchaseReceipt;
                var difference =
                    DateTimeExtensions.DifferenceBetweenTwoUnixTime(
                        prodReceipt.subscriptionExpirationDate.ToUnixTime(),
                        DateTimeExtensions.GetCurrentUnixTime());
                /*if (difference > 0)
                {
                    _isRestoreAllowed = true;
                    LocalDataManager.Instance.SaveData.SubscriptionDetails.SafeAdd("Title", GetProduct(prodReceipt.productID).metadata.localizedTitle);
                    LocalDataManager.Instance.SaveData.SubscriptionDetails.SafeAdd("Date", productReceipt.purchaseDate.ToShortDateString());
                    Debug.Log("  LocalDataManager.Instance.SaveData.NonConsumableBuy Added");
                    break;
                }*/
                 
                 LocalDataManager.Instance.SaveData.NonConsumableBuy.AddUnique(GetProduct(prodReceipt.productID).metadata.localizedTitle);
                 Debug.Log("  LocalDataManager.Instance.SaveData.NonConsumableBuy Added");
            
            }
#endif
            }
            catch (IAPSecurityException e)
            {
                Debug.Log("Error on Re-validating " + e.Message);

                //clear all saved purchased ids, if there is validator error
                LocalDataManager.Instance.SaveData.NonConsumableBuy.Clear();
            }
        }

        public bool IsInitialized()
        {
#if !UNITY_EDITOR
        return StoreController != null && StoreExtensionProvider != null;
#elif UNITY_EDITOR
            return FailInEditor; //to test editor for non-premium user, make this return true
#endif
        }

        public void BuyProduct(string productID)
        {
            UILoader.Instance.StartLoader();
            BuyProductId(productID);
        }


        void BuyProductId(string productId)
        {
            if (IsInitialized())
            {
#if !UNITY_EDITOR
            Product product = StoreController.products.WithID (productId);
            if (product != null && product.availableToPurchase) {
                //Debug.Log (string.Format ("Purchasing product asynchronously: '{0}'", product.definition.id));
                StoreController.InitiatePurchase (product);
            } else {
                UILoader.Instance.StopLoader (true);
                OnPurchaseFailure.SafeInvoke (product, PurchaseFailureReason.ProductUnavailable);
                //Debug.Log ("BuyProductId: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
#elif UNITY_EDITOR
                if (FailInEditor)
                {
                    UILoader.Instance.StopLoader(true);
                    OnPurchaseFailure.SafeInvoke(null, PurchaseFailureReason.ProductUnavailable);
                    Debug.Log("BuyProductId: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
                else
                {
                    Debug.Log(string.Format("Purchasing product asynchronously: '{0}'", productId));
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productId));
                    OnPurchaseSuccess.SafeInvoke(null);
                    UILoader.Instance.StopLoader(true);
                }
#endif
            }
            else
            {
                UILoader.Instance.StopLoader(true);
                OnPurchaseFailure.SafeInvoke(null, PurchaseFailureReason.Unknown);
                Debug.Log("BuyProductId FAIL. Not initialized.");
            }
        }

        public void RestorePurchases(Action onSuccess, Action onFailure)
        {
            UILoader.Instance.StartLoader();
            if (!IsInitialized())
            {
                //Debug.Log ("RestorePurchases FAIL. Not initialized.");
                onFailure.SafeInvoke();
                UILoader.Instance.StopLoader(true);
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                //Debug.Log ("RestorePurchases started ...");

                var apple = StoreExtensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions((result) =>
                {
                    //				//Debug.Log ("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");

                    if (result)
                    {
                        if (_isRestoreAllowed)
                        {
                            _isRestoreAllowed = false;
                            onSuccess.SafeInvoke();
                            UIPopupBox.Instance.SetDataOk("Restoration Successful!", null);
                        }
                        else
                        {
                            onFailure.SafeInvoke();
                            UIPopupBox.Instance.SetDataOk("No valid purchase available in this account to restore!!", null);
                        }

                        UILoader.Instance.StopLoader(true);
                    }
                    else
                    {
                        onFailure.SafeInvoke();
                        UILoader.Instance.StopLoader(true);
                    }
                });
            }
            else
            {
                //Debug.Log ("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
                onFailure.SafeInvoke();
                UILoader.Instance.StopLoader(true);
            }
        }


        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
#if !UNITY_EDITOR
        StoreController = controller;
        StoreExtensionProvider = extensions;
        RevalidateSubscription();
        OnInitSuccess.SafeInvoke ();
#elif UNITY_EDITOR
            if (FailInEditor)
            {
                //Debug.Log ("OnInitializeFailed InitializationFailureReason:" + "Fail In Editor");
                OnInitFailure.SafeInvoke();
            }
            else
            {
                //Debug.Log ("OnInitialized: PASS");
                StoreController = controller;
                StoreExtensionProvider = extensions;
                OnInitSuccess.SafeInvoke();
            }
#endif

            _isInitializing = false;

            Debug.Log("OnInitialized");
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            //Debug.Log ("OnInitializeFailed InitializationFailureReason:" + error);
            OnInitFailure.SafeInvoke();
            _isInitializing = false;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log("ProcessPurchase");
            try
            {
                var product = StoreController.products.WithID(args.purchasedProduct.definition.id);
#if UNITY_ANDROID
                Debug.Log(" Validating Google Receipt");
                var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                    null, Application.identifier);
                var result = validator.Validate(args.purchasedProduct.receipt);

                switch (product.definition.type)
                {
                    case ProductType.Consumable:
                        //no need to store consumable values
                        break;
                    case ProductType.NonConsumable:
                    {
                        if (product.hasReceipt)
                        {
                            _isRestoreAllowed = true;
                            LocalDataManager.Instance.SaveData.NonConsumableBuy.AddUnique(product.metadata.localizedTitle);
                        }
                        else
                        {
                            _isRestoreAllowed = false;
                            LocalDataManager.Instance.SaveData.NonConsumableBuy.Remove(product.metadata.localizedTitle);
                        }
                    }
                        break;
                    case ProductType.Subscription:
                    {
                        //todo: if there is subscription, use subscription date too
                        /*if (product.hasReceipt)
                    {
                        _isRestoreAllowed = true;
                        LocalDataManager.Instance.SaveData.NonConsumableBuy.AddUnique(product.metadata.localizedTitle);
                    }
                    else
                    {
                        _isRestoreAllowed = false;
                        LocalDataManager.Instance.SaveData.NonConsumableBuy.Remove(product.metadata.localizedTitle);
                    }*/
                    }
                        break;
                }

                //todo: refactor for ios, as above
#elif UNITY_IOS
             Debug.Log(" Validating iOS Receipt");
            var validator = new CrossPlatformValidator(null,
               AppleTangle.Data(), Application.identifier);
            string appReceipt = appleConfig.appReceipt;
            string unityIAPReceipt = "{ \"Store\": \"AppleAppStore\", \"Payload\": \"" + appReceipt + "\" }";
            var result = validator.Validate(unityIAPReceipt);
    
             _isRestoreAllowed = false;
            LocalDataManager.Instance.SaveData.NonConsumableBuy.Clear();
            Debug.Log("  LocalDataManager.Instance.SaveData.NonConsumableBuy Cleared");

            foreach (IPurchaseReceipt productReceipt in result)
            {
                var prodReceipt = productReceipt as AppleInAppPurchaseReceipt;
                var difference =
                    DateTimeExtensions.DifferenceBetweenTwoUnixTime(
                        prodReceipt.subscriptionExpirationDate.ToUnixTime(),
                        DateTimeExtensions.GetCurrentUnixTime());
                /*if (difference > 0)
                {
                    LocalDataManager.Instance.SaveData.SubscriptionDetails.SafeAdd("Title", product.metadata.localizedTitle);
                    LocalDataManager.Instance.SaveData.SubscriptionDetails.SafeAdd("Date", productReceipt.purchaseDate.ToShortDateString());
                    Debug.Log(" LocalDataManager.Instance.SaveData.SubscriptionDetails Added");
                    break;
                }*/
    
                _isRestoreAllowed = true;
                LocalDataManager.Instance.SaveData.NonConsumableBuy.AddUnique(GetProduct(prodReceipt.productID).metadata.localizedTitle);
                Debug.Log("LocalDataManager.Instance.SaveData.NonConsumableBuy Added");
            }
#endif
            }
            catch (IAPSecurityException)
            {
                Debug.Log("Invalid receipt for " + args.purchasedProduct.definition.id + ", not unlocking content");
                LocalDataManager.Instance.SaveData.NonConsumableBuy.Clear();
            }

            OnPurchaseSuccess.SafeInvoke(args.purchasedProduct);
            UILoader.Instance.StopLoader(true);
            return PurchaseProcessingResult.Complete;
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
            OnPurchaseFailure.SafeInvoke(product, failureReason);
            UILoader.Instance.StopLoader(true);
        }

        #region Utilities

        public List<Product> GetProducts()
        {
            var products = new List<Product>();
            if (IsInitialized() && StoreController != null)
            {
                return StoreController.products.all.ToList();
            }

            Debug.Log("GetProducts::Store Initialization error");
            return products;
        }

        public Product GetProduct(string id)
        {
            if (IsInitialized() && StoreController != null)
            {
                return StoreController.products.all.ToList().Find(x => x.definition.id == id || x.definition.storeSpecificId == id);
            }

            return null;
        }

        /// <summary>
        /// This includes both non-consumable and subscription purchases
        /// </summary>
        /// <returns></returns>
        public List<Product> GetNonConsumablePurchasedProducts()
        {
            var allProducts = GetProducts();
            var purchasedProducts = allProducts.Where(x => x.hasReceipt).ToList();

            return purchasedProducts;
        }

        //removing static references on manager destroy -> cause when app resets
        protected override void RefreshStaticOnDestroy()
        {
            StoreController = null;
            StoreExtensionProvider = null;
            OnPurchaseSuccess = null;

            OnInitSuccess = null;
            OnInitFailure = null;
            OnPurchaseFailure = null;

            PremiumProduct = null;
            PremiumProductReceipt = null;
        }

        #endregion
    }
}

#endif