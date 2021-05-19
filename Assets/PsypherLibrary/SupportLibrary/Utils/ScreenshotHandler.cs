using PsypherLibrary.SupportLibrary.Utils.FileManager;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils
{
    public class ScreenshotHandler : GenericSingleton<ScreenshotHandler>
    {
        #region fields and properties

        [SerializeField] [Tooltip("getting pixel data from this camera")]
        private Camera _screenshotCamera;

        private bool _takeScreenshotNextFrame = false;

        private string _relativeSavePath;

        #endregion

        #region initialization

        protected override void Awake()
        {
            base.Awake();

            if (_screenshotCamera == null)
            {
                _screenshotCamera = Camera.main;
            }

            if (string.IsNullOrEmpty(_relativeSavePath))
            {
                _relativeSavePath = "CameraScreenshots";
            }
        }

        #endregion

        #region Engine Event

        private void OnRenderObject()
        {
            if (!_takeScreenshotNextFrame) return;

            _takeScreenshotNextFrame = false;
            var renderTexture = _screenshotCamera.targetTexture;

            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            var finalPath = FileStaticAPI.CreateFileIncrementInPersistant(_relativeSavePath + "/CameraScreenshot", "png");
            Debug.Log("final Path:: " + finalPath);

            byte[] byteArray = renderResult.EncodeToPNG();
            FileStaticAPI.WriteInPersistant(finalPath, byteArray);
            Debug.Log("Screenshot Handler::: Success  - Path -> " + finalPath);

            RenderTexture.ReleaseTemporary(renderTexture);
            _screenshotCamera.targetTexture = null;
        }

        #endregion

        #region Actions

        public void SetScreenshotCamera(Camera cam)
        {
            _screenshotCamera = cam;
        }

        public void TakeScreenshot(int width = 500, int height = 500)
        {
            _screenshotCamera.targetTexture = RenderTexture.GetTemporary(width, height);
            _takeScreenshotNextFrame = true;

            Debug.Log("Screenshot Handler:: process start");
        }

        public void SetRelativeSavePath(string path)
        {
            _relativeSavePath = path;
        }

        #endregion
    }
}