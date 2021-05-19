using System;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts
{
    [RequireComponent(typeof(Image))]
    public class UIRemoteImage : UIPanel
    {
        string _imageUrl = string.Empty;
        public bool isLoadOnRecieveUrl = false;
        public bool isLoadOnVisible = true;
        public bool isPictureLoaded = false;
        public Action OnPictureLoaded;

        public string ImageURL
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
                if (isLoadOnRecieveUrl)
                    LoadImage();
            }
        }

        Image image;

        public Image DynamicImage
        {
            get
            {
                if (image == null)
                    image = GetComponent<Image>();

                return image;
            }
            set { image = value; }
        }

        public override void OnVisible()
        {
            if (isLoadOnVisible)
                Invoke("LoadImage", 1.5f);
        }

        //so that it always fetch the latest image
        protected override void OnEnable()
        {
            base.OnEnable();

            Invoke("LoadImage", 1.5f);
        }

        void LoadImage()
        {
            if (!string.IsNullOrEmpty(ImageURL))
            {
                FileManager.Instance.GetImage(urls: _imageUrl, onComplete: (texture) =>
                {
                    if (texture[0].GetRawTextureData().LongLength < 1000)
                    {
                        Debug.Log("Texture Size too small");
                        return;
                    }

                    DynamicImage.SetImage(texture[0].GetSprite());
                }, owner: this, onFail: () => { Debug.Log("Fail to download : " + ImageURL); });
            }
        }

        public override void OnInvisible()
        {
            if (!string.IsNullOrEmpty(_imageUrl))
            {
                CancelInvoke("LoadImage");
                FileManager.CancelDownload(_imageUrl);
            }
        }


        protected override void OnDisable()
        {
            base.OnDisable();

            FileManager.RemoveReference(this);
            IsVisible = false;
        }
    }
}