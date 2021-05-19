using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEngine;
using UnityEngine.Networking;

namespace PsypherLibrary.SupportLibrary.Managers
{
    public enum EFileType
    {
        [Description(".avi|.mp4")]
        Video,

        [Description(".mp3")]
        Audio,

        [Description(".png|.jpg")]
        Image,

        [Description(".json")]
        Data,

        [Description(".")]
        Invalid
    }

    public class FMDownloadObject
    {
        Action<float> ProgressCallback;
        object OnCompleteCallback;
        Action OnFailureCallback;
        readonly List<UnityWebRequest> Requests = new List<UnityWebRequest>();
        readonly List<Coroutine> Coroutines = new List<Coroutine>();
        public string Id = String.Empty;
        readonly bool NeedDataOnComplete = true;
        public Action<FMDownloadObject> OnDownloadComplete;
        readonly List<string> Urls = new List<string>();
        public bool IsDownloadStarted = false;
        private int downloadCounter = 0;

        public UnityEngine.Object Owner;

        public static FMDownloadObject CreateRequest(List<string> urls, object onCompleteCallback, UnityEngine.Object owner, string id, Action<float> progressCallback = null,
            Action onFailureCallback = null, bool needDataOnComplete = true)
        {
            return new FMDownloadObject(urls, onCompleteCallback, progressCallback, onFailureCallback, needDataOnComplete, owner, id);
        }

        public FMDownloadObject StartDownload()
        {
            if (!IsDownloadStarted)
            {
                IsDownloadStarted = true;

                foreach (var req in Requests)
                {
                    Coroutines.Add(FileManager.Instance.StartCoroutine(BatchDownload(req)));
                }

                Coroutines.Add(FileManager.Instance.StartCoroutine(UpdateBatchProgress()));
            }

            return this;
        }

        FMDownloadObject(List<string> urls, object onCompleteCallback, Action<float> progressCallback, Action onFailureCallback, bool needDataOnComplete, UnityEngine.Object owner, string id)
        {
            this.Id = id;
            this.ProgressCallback = progressCallback;
            this.OnCompleteCallback = onCompleteCallback;
            this.OnFailureCallback = onFailureCallback;
            this.NeedDataOnComplete = needDataOnComplete;
            this.Owner = owner;
            Urls.AddRange(urls.Select(x => new Uri(x).AbsoluteUri));
            var randPool = Guid.NewGuid().GetMD5CheckSum();
            for (int i = 0, j = 0; i < Urls.Count; i++)
            {
                if (!FileManager.IsFileDownloaded(Urls[i]))
                {
                    if (j > randPool.Length - 1)
                        j = 0;

                    Urls[i] = String.Concat(Urls[i], "?p=" + randPool[j++]);
                    Requests.Add(UnityWebRequest.Get(new Uri(Urls[i]).AbsoluteUri));
                }
            }
        }

        IEnumerator BatchDownload(UnityWebRequest www)
        {
            yield return www.SendWebRequest();

#if UNITY_5
        if (www.isError)

#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.Log(string.Format("Download Failed for Url ->  {0}  , Error  -> {1}", www.url,
                    www.error));
                OnFailure();
            }
            else
                OnBatchComplete(www);
        }

        IEnumerator UpdateBatchProgress()
        {
            while (true)
            {
                ProgressCallback.SafeInvoke((int) ((Requests.Select(dl =>
                    dl.downloadProgress >= 0 ? dl.downloadProgress : 0
                ).Sum() / Requests.Count) * 100));
                yield return null;
            }
        }

        void OnBatchComplete(UnityWebRequest www)
        {
            FileManager.SaveToCache(www);
            if (IsBatchDownloadOver())
            {
                Coroutines.ForEach(x => FileManager.Instance.StopCoroutine(x));
                Coroutines.Clear();
                ProgressCallback.SafeInvoke(100f);
                var url = www.url.Split('?')[0];
                for (int i = 0; i < Urls.Count; i++)
                {
                    Urls[i] = Urls[i].Split('?')[0];
                }

                switch (FileManager.GetFileType(url))
                {
                    case EFileType.Audio:
                    {
                        var audioClips = new List<AudioClip>();
                        if (NeedDataOnComplete)
                        {
                            Urls.ForEach((x) =>
                            {
                                FileManager.Instance.StartCoroutine(FileManager.LoadAudioFromCache(FileManager.GetFilePath(x), (y) =>
                                {
                                    audioClips.Add(y);
                                    if (audioClips.Count == Urls.Count)
                                    {
                                        if (OnCompleteCallback != null)
                                            ((Action<List<AudioClip>>) OnCompleteCallback).SafeInvoke(audioClips);
                                        OnDownloadComplete.SafeInvoke(this);
                                    }
                                }));
                            });
                        }
                        else
                        {
                            if (OnCompleteCallback != null)
                                ((Action<List<AudioClip>>) OnCompleteCallback).SafeInvoke(audioClips);
                            OnDownloadComplete.SafeInvoke(this);
                        }


                        break;
                    }
                    case EFileType.Video:
                    {
                        if (NeedDataOnComplete)
                        {
                            var videoUrls = Urls.Select(x => "file://" + FileManager.GetFilePath(x)).ToList();
                            if (OnCompleteCallback != null)
                                ((Action<List<string>>) OnCompleteCallback).SafeInvoke(videoUrls);
                        }
                        else
                        {
                            if (OnCompleteCallback != null)
                                ((Action<List<string>>) OnCompleteCallback).SafeInvoke(new List<string>());
                        }

                        OnDownloadComplete.SafeInvoke(this);
                        break;
                    }
                    case EFileType.Image:
                    {
                        if (NeedDataOnComplete)
                        {
                            var images = Urls.Select(FileManager.LoadImageFromCache).ToList();
                            if (OnCompleteCallback != null)
                                ((Action<List<Texture2D>>) OnCompleteCallback).SafeInvoke(images);
                        }
                        else
                        {
                            if (OnCompleteCallback != null)
                                ((Action<List<Texture2D>>) OnCompleteCallback).SafeInvoke(new List<Texture2D>());
                        }

                        OnDownloadComplete.SafeInvoke(this);
                        break;
                    }
                    case EFileType.Data:
                    {
                        if (NeedDataOnComplete)
                        {
                            var allData = Urls.Select(x =>
                            {
                                var json = PiUtilities.LoadJsonData(x.GetUniqueId(),
                                    Path.Combine(PiUtilities.SavePath, FileManager.GetFileType(x).ToString()), "." + FileManager.GetFileFormat(x));

                                return json;
                            }).ToList();
                            if (OnCompleteCallback != null)
                                ((Action<List<string>>) OnCompleteCallback).SafeInvoke(allData);
                        }
                        else
                        {
                            if (OnCompleteCallback != null)
                                ((Action<List<string>>) OnCompleteCallback).SafeInvoke(new List<string>());
                        }

                        OnDownloadComplete.SafeInvoke(this);
                        break;
                    }
                }
            }
        }

        public void OnFailure()
        {
            ProgressCallback.SafeInvoke(0f);
            Coroutines.ForEach(x => FileManager.Instance.StopCoroutine(x));
            Requests.ForEach(x =>
            {
                x.Abort();
                x.Dispose();
            });
            Coroutines.Clear();
            OnFailureCallback.SafeInvoke();
            NullCallBacks();
            OnDownloadComplete.SafeInvoke(this);
        }

        public void NullCallBacks()
        {
            OnCompleteCallback = OnFailureCallback = null;
        }

        public FMDownloadObject AppendDownload(string url, object onCompleteCallback, Action<float> progressCallback = null,
            Action onFailureCallback = null)
        {
            switch (FileManager.GetFileType(url))
            {
                case EFileType.Audio:
                {
                    var callback = ((Action<List<AudioClip>>) this.OnCompleteCallback);
                    callback += ((Action<List<AudioClip>>) onCompleteCallback);
                    OnCompleteCallback = callback;
                    ProgressCallback += progressCallback;
                    OnFailureCallback = onFailureCallback;
                    break;
                }
                case EFileType.Video:
                {
                    var callback = ((Action<List<string>>) this.OnCompleteCallback);
                    callback += ((Action<List<string>>) onCompleteCallback);
                    OnCompleteCallback = callback;
                    ProgressCallback += progressCallback;
                    OnFailureCallback = onFailureCallback;
                    break;
                }
                case EFileType.Image:
                {
                    var callback = ((Action<List<Texture2D>>) this.OnCompleteCallback);
                    callback += ((Action<List<Texture2D>>) onCompleteCallback);
                    OnCompleteCallback = callback;
                    ProgressCallback += progressCallback;
                    OnFailureCallback = onFailureCallback;
                    break;
                }
                case EFileType.Data:
                {
                    var callback = ((Action<List<string>>) this.OnCompleteCallback);
                    callback += ((Action<List<string>>) onCompleteCallback);
                    OnCompleteCallback = callback;
                    ProgressCallback += progressCallback;
                    OnFailureCallback = onFailureCallback;
                    break;
                }
            }

            return this;
        }

        public bool IsBatchDownloadOver()
        {
            return (++downloadCounter == Requests.Count);
        }
    }

    public class FileManager : GenericManager<FileManager>
    {
        const long VideoCacheLimit = 1024 * 1024;

        //1GB
        const long ImageCacheLimit = 1024 * 1024;
        const long AudioCacheLimit = 1024 * 1024;
        const long DataCacheLimit = (512 * 512);
        private const int Texture2dCacheLimit = 100;
        static KeyValuePair<string, Texture2D>[] ImageCache2 = new KeyValuePair<string, Texture2D>[Texture2dCacheLimit];
        private static int ImageCache2Index = 0;

        private static int ImageCache2Dir = 1;

        //256mb
        static readonly Dictionary<string, FMDownloadObject> CurrentDownloads = new Dictionary<string, FMDownloadObject>();
        static readonly Dictionary<string, List<string>> DownloadReferences = new Dictionary<string, List<string>>();
#if UNITY_EDITOR
        public static PiUtilities.ESaveTypes DataSaveType = PiUtilities.ESaveTypes.Readable;
#else
        public static PiUtilities.ESaveTypes DataSaveType = PiUtilities.ESaveTypes.Unreadable;
#endif

        public static void Empty(DirectoryInfo directory)
        {
            if (directory.Exists)
            {
                foreach (System.IO.FileInfo file in directory.GetFiles())
                    file.Delete();

                foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories())
                    subDirectory.Delete(true);
            }
        }

        public static void RemoveFolder(EFileType folderType)
        {
            var dirInfo = (new DirectoryInfo(PiUtilities.SavePath));
            if (dirInfo.Exists)
            {
                var dirs = dirInfo.GetDirectories();
                foreach (var dir in dirs)
                {
                    if (string.Equals(dir.Name, folderType.ToString()))
                    {
                        foreach (var file in dir.GetFiles())
                        {
                            file.Delete();
                        }

                        dir.Delete();
                        return;
                    }
                }
            }
        }

        public static void RemoveAllData()
        {
            //  PlayerPrefs.DeleteAll();
            Empty(new DirectoryInfo(PiUtilities.SavePath));
        }

        public static EFileType GetFileType(string url)
        {
            Uri uri = new Uri(url);
            var fileType = uri.Segments[uri.Segments.Length - 1].Split('.');
            var values = Enum.GetValues(typeof(EFileType));
            foreach (EFileType val in values)
            {
                var formats = val.GetDescription().Split('|');
                foreach (var format in formats)
                {
                    if (string.Equals(format.Replace(".", ""), fileType.Length > 0 ? Uri.UnescapeDataString(fileType[1]) : Uri.UnescapeDataString(fileType[0])))
                        return val;
                }
            }

            return EFileType.Invalid;
        }

        public static string GetFileFormat(string url)
        {
            Uri uri = new Uri(url);
            var fileType = uri.Segments[uri.Segments.Length - 1].Split('.');
            return fileType.Length > 0 ? Uri.UnescapeDataString(fileType[1]) : Uri.UnescapeDataString(fileType[0]);
        }

        public static string GetFilePath(string fileUrl)
        {
            fileUrl = new Uri(fileUrl).AbsoluteUri;
            return Uri.UnescapeDataString(Path.Combine(Path.Combine(PiUtilities.SavePath, GetFileType(fileUrl).ToString()), fileUrl.GetUniqueId() + "." + GetFileFormat(fileUrl)));
        }

        public static Texture2D LoadImageFromCache(string fileUrl)
        {
            var texture2D = LoadImageFromCache2(fileUrl);
            if (texture2D != null)
                return texture2D;

            var id = fileUrl.GetUniqueId();
            var bytes = File.ReadAllBytes(GetFilePath(fileUrl));
            var tex = new Texture2D(1, 1);
            tex.LoadImage(bytes);
            tex.name = GetFileName(fileUrl);


            ImageCache2[ImageCache2Index] = new KeyValuePair<string, Texture2D>(id, tex);
            if (ImageCache2Index + ImageCache2Dir > ImageCache2.Length - 1)
                ImageCache2Dir = -1;
            if (ImageCache2Index + ImageCache2Dir < 0)
                ImageCache2Dir = 1;
            ImageCache2Index += ImageCache2Dir;

            return tex;
        }

        private static Texture2D LoadImageFromCache2(string fileUrl)
        {
            var id = fileUrl.GetUniqueId();
            for (int i = ImageCache2.Length - 1; i >= 0; i--)
            {
                if (string.Equals(ImageCache2[i].Key, id))
                    return ImageCache2[i].Value;
            }

            return null;
        }

        public static void UpdateCache(string id, Texture2D texture)
        {
            for (int i = ImageCache2.Length - 1; i >= 0; i--)
            {
                if (string.Equals(ImageCache2[i].Key, id))
                    ImageCache2[i] = new KeyValuePair<string, Texture2D>(id, texture);
            }
        }

        public static IEnumerator LoadAudioFromCache(string fileUrl, Action<AudioClip> onLoad, Action onFail = null, AudioType type = AudioType.MPEG)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
                fileUrl = "file:///" + fileUrl;
            else
                fileUrl = "file://" + fileUrl;
            using (var www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, type))
            {
                yield return www;

                if (string.IsNullOrEmpty(www.error))
                {
                    var aud = DownloadHandlerAudioClip.GetContent(www);
                    aud.name = GetFileName(fileUrl);
                    onLoad.SafeInvoke(aud);
                }
                else
                {
                    onFail.SafeInvoke();
                }
            }
        }

        static double GetDirectorySizeInMb(string path)
        {
            if (!Directory.Exists(PiUtilities.SavePath))
                return 0;

            string[] files = Directory.GetFiles(path);
            string[] subdirectories = Directory.GetDirectories(path);

            double size = files.Sum(x => new FileInfo(x).Length);
            foreach (string s in subdirectories)
                size += GetDirectorySizeInMb(s);


            size = size / 1048576;

            return size;
        }

        static bool IsCacheFull(EFileType fileType, byte[] incomingFile)
        {
            double size = GetDirectorySizeInMb(PiUtilities.SavePath + fileType) + (incomingFile.Length / (double) (1024 * 1024));
            long cacheLimit = 0;
            switch (fileType)
            {
                case EFileType.Audio:
                {
                    cacheLimit = AudioCacheLimit;
                    break;
                }
                case EFileType.Video:
                {
                    cacheLimit = VideoCacheLimit;
                    break;
                }
                case EFileType.Image:
                {
                    cacheLimit = ImageCacheLimit;
                    break;
                }
                case EFileType.Data:
                {
                    cacheLimit = DataCacheLimit;
                    break;
                }
            }

            return size >= cacheLimit;
        }

        static void MakeSpaceForFile(EFileType fileType, byte[] fileBytes)
        {
            if (Directory.Exists(PiUtilities.SavePath + fileType) && IsCacheFull(fileType, fileBytes))
            {
                Action<IEnumerable<FileInfo>> remover = (input) =>
                {
                    foreach (var file in input)
                    {
                        File.Delete(file.FullName);
                    }
                };

                var dir = new DirectoryInfo(PiUtilities.SavePath + fileType.ToString());

                List<FileInfo> files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly).ToList();
                files.Sort((a, b) => a.LastAccessTimeUtc.CompareTo(b.LastAccessTimeUtc));
                int i = 0;
                while (IsCacheFull(fileType, fileBytes))
                {
                    if (i >= files.Count)
                        return;
                    remover.Invoke(files.Skip(i).Take(1));
                    i++;
                }
            }
        }

        public static void SaveToCache(UnityWebRequest www)
        {
            var origUrl = www.url.Split('?')[0];
            if (!Directory.Exists(Path.Combine(PiUtilities.SavePath, GetFileType(origUrl).ToString())))
            {
                Directory.CreateDirectory(Path.Combine(PiUtilities.SavePath, GetFileType(origUrl).ToString()));
            }

            //MakeSpaceForFile(GetFileType(www.url), www.downloadHandler.data);
            if (GetFileType(origUrl) == EFileType.Data)
            {
                www.downloadHandler.text.SaveJson(origUrl.GetUniqueId(), Path.Combine(PiUtilities.SavePath, GetFileType(origUrl).ToString()), "." + GetFileFormat(origUrl));
            }
            else
            {
                using (var file = File.Open(GetFilePath(origUrl), FileMode.Create))
                using (var binary = new BinaryWriter(file))
                {
                    binary.Write(www.downloadHandler.data);
                }
            }
        }

        public static bool IsFileDownloaded(string url)
        {
            return File.Exists(GetFilePath(url));
        }

        public static void RemoveFile(string url)
        {
            if (IsFileDownloaded(url))
                File.Delete(GetFilePath(url));
        }

        public static bool IsFileDownloaded(IEnumerable<string> urls)
        {
            bool result = true;
            foreach (var url in urls)
            {
                result = IsFileDownloaded(url);
                if (!result)
                    break;
            }

            return result;
        }

        public static bool IsDownloadInitiated(params string[] urls)
        {
            if (urls.Length > 0)
            {
                string id = String.Empty;
                if (urls.Length == 1)
                {
                    id = new Uri(urls[0]).AbsoluteUri.GetUniqueId();
                }
                else
                {
                    id = urls.ToList().GetUniqueId(); //urls.ToList().Aggregate((i, j) => new Uri(i).AbsoluteUri + new Uri(j).AbsoluteUri).GenerateUniqueId();
                }

                return CurrentDownloads.ContainsKey(id);
            }

            return false;
        }

        public static bool CancelDownload(params string[] urls)
        {
            if (urls.Length > 0)
            {
                string id = String.Empty;
                if (urls.Length == 1)
                {
                    id = new Uri(urls[0]).AbsoluteUri.GetUniqueId();
                }
                else
                {
                    id = urls.ToList().GetUniqueId(); // urls.ToList().Aggregate((i, j) => new Uri(i).AbsoluteUri + new Uri(j).AbsoluteUri).GenerateUniqueId();
                }

                if (CurrentDownloads.ContainsKey(id))
                {
                    CurrentDownloads[id].OnFailure();
                    return true;
                }
            }

            return false;
        }

        public static void SaveImageToGallery(string fileName, byte[] byteStream)
        {
            var location = "/mnt/sdcard/DCIM/" + Application.productName;


            PiUtilities.SetEnvironmentVariableForSerialization();

            if (!Directory.Exists(location))
            {
                Directory.CreateDirectory(location);
            }

            using (var file = File.Open(Path.Combine(location, fileName + ".jpg"), FileMode.Create))
            using (var binary = new BinaryWriter(file))
            {
                binary.Write(byteStream);
            }
        }

        public static string GetFileName(string fileUrl)
        {
            Uri uri = new Uri(fileUrl);
            var fileName = uri.Segments[uri.Segments.Length - 1].Split('.');
            return Uri.UnescapeDataString(fileName[0]);
        }

        bool GetFile(UnityEngine.Object owner, object onComplete, Action onFail = null, Action<float> onProgress = null, bool needDataOnDownload = true, params string[] urls)
        {
            if (urls.Length <= 0)
            {
                onFail.SafeInvoke();
                return false;
            }

            string id = string.Empty;
            var urlList = urls.ToList().Where(x => x != string.Empty).ToList();
            if (urlList.Count == 1)
            {
                id = new Uri(urlList[0]).AbsoluteUri.GetUniqueId();
                if (GetFileType(urlList[0]) == EFileType.Invalid)
                {
                    onFail.SafeInvoke();
                    return false;
                }
            }
            else
            {
                id = urlList.GetUniqueId(); //urlList.Aggregate((i, j) => new Uri(i).AbsoluteUri + new Uri(j).AbsoluteUri).GenerateUniqueId();

                foreach (var url in urlList)
                {
                    if (GetFileType(url) == EFileType.Invalid)
                    {
                        onFail.SafeInvoke();
                        return false;
                    }
                }
            }

            List<string> toDownload = new List<string>();
            foreach (var url in urlList)
            {
                if (!IsFileDownloaded(url))
                {
                    toDownload.Add(url);
                }
            }

            if (toDownload.Any())
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    onFail.SafeInvoke();
                    return false;
                }

                var instanceId = owner.GetInstanceID().ToString();
                if (DownloadReferences.ContainsKey(instanceId))
                    DownloadReferences[instanceId].Add(id);
                else
                    DownloadReferences.Add(instanceId, new List<string> {id});

                if (!CurrentDownloads.ContainsKey(id))
                {
                    CurrentDownloads.Add(id, FMDownloadObject.CreateRequest(urlList.ToList(), onComplete, owner, id, onProgress, onFail, needDataOnDownload).StartDownload());
                    CurrentDownloads[id].OnDownloadComplete += OnComplete;
                }
                else
                {
                    CurrentDownloads[id].AppendDownload(urlList[0], onComplete, onProgress, onFail);
                }
            }

            return !toDownload.Any();
        }


        public void GetData(Action<List<string>> onComplete, UnityEngine.Object owner, Action onFail = null, Action<float> onProgress = null, bool needDataOnComplete = true, params string[] urls)
        {
            if (GetFile(owner, onComplete, onFail, onProgress, needDataOnComplete, urls))
            {
                onProgress.SafeInvoke(100);
                onComplete.SafeInvoke(needDataOnComplete ? urls.Select(x => PiUtilities.LoadJsonData(x.GetUniqueId(), Path.Combine(PiUtilities.SavePath, GetFileType(x).ToString()), "." + GetFileFormat(x))).ToList() : null);
            }
        }


        public void GetImage(Action<List<Texture2D>> onComplete, UnityEngine.Object owner, Action onFail = null, Action<float> onProgress = null, bool needDataOnComplete = true, params string[] urls)
        {
            if (GetFile(owner, onComplete, onFail, onProgress, needDataOnComplete, urls))
            {
                onProgress.SafeInvoke(100);
                onComplete.SafeInvoke(needDataOnComplete ? urls.Select(LoadImageFromCache).ToList() : null);
            }
        }


        public void GetVideo(Action<List<string>> onComplete, UnityEngine.Object owner, Action onFail = null, Action<float> onProgress = null, bool needDataOnComplete = true, params string[] urls)
        {
            if (GetFile(owner, onComplete, onFail, onProgress, needDataOnComplete, urls))
            {
                onProgress.SafeInvoke(100);
                onComplete.SafeInvoke(needDataOnComplete ? urls.Select(x => "file://" + GetFilePath(x)).ToList() : null);
            }
        }


        public void GetAudio(Action<List<AudioClip>> onComplete, UnityEngine.Object owner, Action onFail = null, Action<float> onProgress = null, bool needDataOnComplete = true, params string[] urls)
        {
            if (GetFile(owner, onComplete, onFail, onProgress, needDataOnComplete, urls))
            {
                onProgress.SafeInvoke(100);
                var audioClips = new List<AudioClip>();
                var totalClips = urls.Length;
                if (needDataOnComplete)
                {
                    urls.Each((x, i) =>
                    {
                        StartCoroutine(LoadAudioFromCache(GetFilePath(urls[i]), (y) =>
                        {
                            audioClips.Add(y);
                            if (audioClips.Count == totalClips)
                            {
                                onComplete.SafeInvoke(audioClips);
                            }
                        }));
                    });
                }
                else
                    onComplete.SafeInvoke(null);
            }
        }

        static void OnComplete(FMDownloadObject obj)
        {
            var id = obj.Id;
            RemoveReference(obj.Owner, obj);
            if (CurrentDownloads.ContainsKey(id))
            {
                CurrentDownloads[id] = null;
                CurrentDownloads.Remove(id);
            }
        }

        public static void RemoveReference(UnityEngine.Object reference, FMDownloadObject obj = null)
        {
            var instanceId = reference.GetInstanceID().ToString();
            if (DownloadReferences.ContainsKey(instanceId))
            {
                if (obj == null)
                {
                    foreach (var id in DownloadReferences[instanceId])
                    {
                        if (CurrentDownloads.ContainsKey(id))
                        {
                            CurrentDownloads[id].NullCallBacks();
                        }
                    }
                }
                else
                {
                    if (CurrentDownloads.ContainsKey(obj.Id))
                    {
                        CurrentDownloads[obj.Id].NullCallBacks();
                    }
                }

                DownloadReferences.Remove(instanceId);
            }
        }
    }
}