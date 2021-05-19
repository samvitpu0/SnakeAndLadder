using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PsypherLibrary.SupportLibrary.BaseProjectSettings;
using PsypherLibrary.SupportLibrary.Utils.FileManager;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Component = UnityEngine.Component;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace PsypherLibrary.SupportLibrary.Extensions
{
    [Serializable]
    public enum EHeaders : int
    {
        [Description("ACCEPT-ENCODING")] AcceptEncoding,

        [Description("CONTENT-ENCODING")] ContentEncoding,

        [Description("CONTENT-TYPE")] ContentType,

        [Description("gzip")] Gzip
    }

    public static class PiUtilities
    {
        public enum ESaveTypes
        {
            Readable,
            Unreadable,
        }

        public static Action OnLoad;
        public static Action OnLoadDone;
        public static ESaveTypes DataSaveType = BaseSettings.Instance.DataSaveType;

        public static string SavePath
        {
            get { return Path.Combine(Application.persistentDataPath, "Save"); }
        }

        public static double _checkCastToDouble;

        public static float GetRandomNumberAround(this float inValue, float up, float down)
        {
            float highRandom = Random.Range(inValue, inValue + up);
            float lowRandom = Random.Range(inValue - down, inValue);

            return Random.Range(lowRandom, highRandom);
        }

        public static string CreateDirectoryMd5(string srcPath)
        {
            var filePaths = Directory.GetFiles(srcPath, "*", SearchOption.AllDirectories).OrderBy(p => p).ToArray();

            using (var md5 = MD5.Create())
            {
                foreach (var filePath in filePaths)
                {
                    // hash path
                    byte[] pathBytes = Encoding.UTF8.GetBytes(filePath);
                    md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                    // hash contents
                    byte[] contentBytes = File.ReadAllBytes(filePath);

                    md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
                }

                //Handles empty filePaths case
                md5.TransformFinalBlock(new byte[0], 0, 0);

                return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
            }
        }

        public static float Fraction(this float num, float percentage)
        {
            return num * (percentage / 100f);
        }

        public static byte[] ObjectToByteArray(this object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        public static object ByteArrayToObject(this byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object obj = (object) binForm.Deserialize(memStream);

            return obj;
        }

        public static void SetEnvironmentVariableForSerialization()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            }
        }

        public static string SaveJson(this string data, string key, string location = null, string extension = null)
        {
            string saveFilePath =
                Path.Combine((location ?? SavePath), key + (extension ?? BaseConstants.SAVE_EXTENSION));
            string saveContent = String.Empty;
            SetEnvironmentVariableForSerialization();
            if (!Directory.Exists(location ?? SavePath))
            {
                Directory.CreateDirectory(location ?? SavePath);
            }

            switch (DataSaveType)
            {
                case ESaveTypes.Readable:
                {
                    File.WriteAllText(saveFilePath, data);
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(saveFilePath))
                        {
                            PlayerPrefs.SetString(key + "MD5", md5.ComputeHash(stream).GetUniqueId());
                        }
                    }


                    saveContent = data;
                    break;
                }
                case ESaveTypes.Unreadable:
                {
                    using (FileStream file = File.Open(saveFilePath, FileMode.OpenOrCreate))
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        var base64String = Convert.ToBase64String(ObjectToByteArray(data));

                        binaryFormatter.Serialize(file, base64String);

                        using (var md5 = MD5.Create())
                        {
                            PlayerPrefs.SetString(key + "MD5", md5.ComputeHash(file).GetUniqueId());
                        }

                        saveContent = base64String;
                    }


                    break;
                }
            }

            return saveContent;
        }

        public static T LoadJsonData<T>(string key, string location = null, string extension = null) where T : class
        {
            var data = LoadJsonData(key, location, extension);
            if (data != null)
                return JsonConvert.DeserializeObject<T>(data);

            return default(T);
        }

        public static string LoadJsonData(string key, string location = null, string extension = null)
        {
            string saveFilePath =
                Path.Combine((location ?? SavePath), key + (extension ?? BaseConstants.SAVE_EXTENSION));

            //saveFilePath = saveFilePath.Replace("\\", "/");

            Debug.Log("Save File Path: " + saveFilePath);

            if (Directory.Exists(location ?? SavePath) && File.Exists(saveFilePath))
            {
                switch (DataSaveType)
                {
                    case ESaveTypes.Readable:
                    {
                        try
                        {
                            var json = File.ReadAllText(saveFilePath);
                            string hash = String.Empty;
                            using (var md5 = MD5.Create())
                            {
                                using (var stream = File.OpenRead(saveFilePath))
                                {
                                    hash = md5.ComputeHash(stream).GetUniqueId();
                                }
                            }

                            if (PlayerPrefs.HasKey(key + "MD5") &&
                                String.Equals(PlayerPrefs.GetString(key + "MD5"), hash))
                                return json.Trim(new char[] {'\uFEFF'});

                            return null;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    case ESaveTypes.Unreadable:
                    {
                        using (FileStream file = File.Open(saveFilePath, FileMode.Open))
                        {
                            try
                            {
                                BinaryFormatter binaryFormatter = new BinaryFormatter();
                                var saveData = binaryFormatter.Deserialize(file);
                                var base64Data = (string) saveData;
                                var actualData = Convert.FromBase64String(base64Data);
                                var objectData = ByteArrayToObject(actualData);

                                string hash = String.Empty;
                                using (var md5 = MD5.Create())
                                {
                                    hash = md5.ComputeHash(file).GetUniqueId();
                                }

                                if (PlayerPrefs.HasKey(key + "MD5") &&
                                    String.Equals(PlayerPrefs.GetString(key + "MD5"), hash))
                                    return objectData.ToString().Trim(new char[] {'\uFEFF'});

                                return null;
                            }
                            catch
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            return null;
        }


        public static List<int> GetIntFromString(this string inString)
        {
            string[] numbers = Regex.Split(inString, @"\D+");
            List<int> recoveredInts = new List<int>();

            foreach (string value in numbers)
            {
                if (!String.IsNullOrEmpty(value))
                {
                    int i = Int32.Parse(value);
                    recoveredInts.Add(i);
                }
            }

            return recoveredInts;
        }

        public static int LimitToRange(
            this int value, int inclusiveMinimum, int inclusiveMaximum)
        {
            if (value < inclusiveMinimum)
            {
                return inclusiveMinimum;
            }

            if (value > inclusiveMaximum)
            {
                return inclusiveMaximum;
            }

            return value;
        }


        public static int ToInt(this string stringVal)
        {
            try
            {
                return Convert.ToInt32(stringVal);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static uint TryParseConvenience(string str, uint failResult)
        {
            return uint.TryParse(str, out var parseResult) ? parseResult : failResult;
        }


        /// <summary>
        /// Assign a new memory stream and copies the data. A safe way to copy lists values, rather than references
        /// </summary>
        public static T Clone<T>(this T objSource)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, objSource);


            memoryStream.Position = 0;
            T returnValue = (T) binaryFormatter.Deserialize(memoryStream);


            memoryStream.Close();
            memoryStream.Dispose();


            return returnValue;
        }

        public static string GetMD5CheckSum(this object input)
        {
            // step 1, calculate MD5 hash from input

            byte[] hash;
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = ObjectToByteArray(input);

                hash = md5.ComputeHash(inputBytes);
            }

            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        //    public static IEnumerator HttpPost(string url, WWWForm form, IServiceCallback callback, bool isBgProcess = false)
        //    {
        //        if (!isBgProcess)
        //            OnLoad.SafeInvoke();
        //        var headers = form.headers;
        //#if UNITY_ANDROID || UNITY_IOS
        //        headers.Add(EHeaders.AcceptEncoding.GetDescription(), EHeaders.Gzip.GetDescription());
        //#endif
        //        WWW www = new WWW(url, form.data, headers);

        //        while (!www.isDone)
        //            yield return null;
        //        if (!isBgProcess)
        //            OnLoadDone.SafeInvoke();
        //        if (string.IsNullOrEmpty(www.error))
        //        {
        //            string datastring = "";
        //            bool replyCompressed = false;
        //            foreach (KeyValuePair<string, string> kvp in www.responseHeaders)
        //            {
        //                if (kvp.Key.ToUpper().Trim().Equals(EHeaders.ContentEncoding.GetDescription()) && kvp.Value.ToLower().Trim().Equals(EHeaders.Gzip.GetDescription()))
        //                {
        //                    replyCompressed = true;
        //                    break;
        //                }
        //            }
        //            if (replyCompressed && www.bytes[0] != '{')
        //            {
        //                datastring = System.Text.Encoding.Default.GetString(GZip.Decompress(www.bytes));
        //            }
        //            else
        //            {
        //                datastring = www.text;
        //            }
        //            Debug.Log("Get From " + url);
        //            Debug.Log("Data from post " + datastring);
        //            callback.OnQuerySuccess(datastring);
        //        }
        //        else
        //        {
        //            Exception e = new Exception(www.error);
        //            Debug.Log("Get From " + url);
        //            Debug.Log("Error in post- " + www.error);
        //            callback.OnQueryException(e);
        //        }
        //    }

        public static IEnumerator HttpDownload(string url,
            Action<DataStorage> OnSuccess, Action<Exception> OnFailure, bool isBgProcess = false)
        {
            if (!isBgProcess)
            {
                OnLoad.SafeInvoke();
            }

            UnityWebRequest www = new UnityWebRequest();
            www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (string.IsNullOrEmpty(www.error))
            {
                DataStorage ds = new DataStorage();
                var bytes = Encoding.UTF8.GetBytes(www.downloadHandler.text);
                var data = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                if (data[0] == '\uFEFF') //Remove BOM // Byte order mark
                {
                    data = data.Substring(1);
                }

                var transportObj = JObject.Parse(data);
                ds.JsonList.Add(transportObj);
                Debug.Log("Get From " + url);
                Debug.Log("Data from post " + data);
                OnSuccess.SafeInvoke(ds);
            }
            else
            {
                OnFailure.SafeInvoke(new Exception(www.error));
            }
        }

        //    public static IEnumerator HttpPost(string url, byte[] postData, Dictionary<string, string> inputHeaders, IServiceCallback callback, bool isBgProcess = false)
        //    {
        //        if (!isBgProcess)
        //            OnLoad.SafeInvoke();


        //        var headers = inputHeaders.Clone();
        //#if UNITY_ANDROID || UNITY_IOS
        //        headers.Add(EHeaders.AcceptEncoding.GetDescription(), EHeaders.Gzip.GetDescription());
        //#endif

        //        //        Debug.LogError("PostData Size Before Compression : " + postData.Length);
        //        //Debug.LogError("PostData Size After Compression : " + GZip.Compress(postData).Length);
        //        WWW www = new WWW(url, postData, headers);
        //        while (!www.isDone)
        //            yield return null;

        //        if (string.IsNullOrEmpty(www.error))
        //        {
        //            string rawData = www.text;


        //            bool replyCompressed = false;
        //            foreach (KeyValuePair<string, string> kvp in www.responseHeaders)
        //            {
        //                if (kvp.Key.ToUpper().Trim().Equals(EHeaders.ContentEncoding.GetDescription()) && kvp.Value.ToLower().Trim().Equals(EHeaders.Gzip.GetDescription()))
        //                {
        //                    replyCompressed = true;
        //                    break;
        //                }
        //            }
        //            if (replyCompressed && www.bytes[0] != '{')
        //            {
        //                rawData = System.Text.Encoding.Default.GetString(GZip.Decompress(www.bytes));
        //            }
        //            Debug.Log("Get From " + url);
        //            Debug.Log("Data from post " + rawData);
        //            callback.OnQuerySuccess(rawData);
        //        }
        //        else
        //        {
        //            Exception e = new Exception(www.error);
        //            Debug.Log("Error in post- " + www.error);
        //            callback.OnQueryException(e);
        //        }

        //        if (!isBgProcess)
        //            OnLoadDone.SafeInvoke();
        //    }

        public static string GetUniqueId(this object data)
        {
            if (data is string) //if the data is a string
            {
                if (BaseSettings.Instance.FileNameSaveType == FileNameType.FileName)
                    return data.ToString(); //returning data as it is
            }

            byte[] uidBytes;
            using (var sha256 = SHA256.Create())
            {
                byte[] inputBytes = ObjectToByteArray(data);

                uidBytes = sha256.ComputeHash(inputBytes);
            }

            var a = default(UInt64);

            int l = 6;
            for (int i = 0; i < l; i++)
            {
                var shift = Math.Abs(Convert.ToInt32((l - i - 1) * 8));
                a |= Convert.ToUInt64(uidBytes[i]) << shift;
            }


            return a.ToString();
        }

        public static void Empty(this DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles())
                file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                subDirectory.Delete(true);
        }

        public static string GetJsonFromCSV(this string value)
        {
            // Get lines.
            if (value == null)
                return null;
            string[] lines = value.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);


            // Get headers.
            string[] headers = lines.First().Split(',');

            // Build JSON array.
            Func<string, string, string> formatString = (string header, string field) =>
            {
                string result;
                try
                {
                    _checkCastToDouble = Convert.ToDouble(field);
                    result = String.Format(" \"{0}\": {1}", header, field);
                }
                catch
                {
                    result = String.Format(" \"{0}\": \"{1}\"", header, field);
                }

                return result;
            };
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            for (int i = 1; i < lines.Length; i++)
            {
                string[] fields = lines[i].Split(',');

                var jsonElements = headers.Zip(fields, (header, field) => formatString.Invoke(header, field)).ToArray();
                string jsonObject = "{" + String.Format("{0}", String.Join(",", jsonElements)) + "}";
                if (i < lines.Length - 1)
                    jsonObject += ",";
                sb.AppendLine(jsonObject);
            }

            sb.AppendLine("]");


            var parsedJson = JsonConvert.DeserializeObject(sb.ToString());
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }


        public static IEnumerable<TResult> Zip<TA, TB, TResult>(
            this IEnumerable<TA> seqA, IEnumerable<TB> seqB, Func<TA, TB, TResult> func)
        {
            if (seqA == null)
                throw new ArgumentNullException("seqA");
            if (seqB == null)
                throw new ArgumentNullException("seqB");

            using (var iteratorA = seqA.GetEnumerator())
            using (var iteratorB = seqB.GetEnumerator())
            {
                while (iteratorA.MoveNext() && iteratorB.MoveNext())
                {
                    yield return func(iteratorA.Current, iteratorB.Current);
                }
            }
        }

        public static SortedDictionary<string, string> GetQueriesFromFields<T>(this T c) where T : class
        {
            var fieldValues = c.GetType().GetFields().ToDictionary(x => x.Name, y => y.GetValue(c).ToString());
            return new SortedDictionary<string, string>(fieldValues);
        }

        /// <summary>
        /// Checks the remote file exists or not.
        /// This method is a bit slow.
        /// </summary>
        public static bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                var request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD
                request.Method = "HEAD";
                //Getting the Web Response.
                var response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }


        //there is one more function like this in InternetChecker which is believed to be better
        public static void InternetStateChange(Action<bool> iStateChangeAction)
        {
            iStateChangeAction.SafeInvoke(NetworkInterface.GetIsNetworkAvailable());
        }

        /// <summary>
        /// Gets the filename from the provived URL
        /// </summary>
        public static string GetFileName(string fileUrl)
        {
            if (String.IsNullOrEmpty(fileUrl)) return String.Empty;

            Uri uri = new Uri(fileUrl);
            Uri reformedUri;
            try
            {
                reformedUri = new Uri(uri.LocalPath);
            }
            catch //if the address is not local use the remote address
            {
                reformedUri = uri;
            }

            var fileName = reformedUri.Segments.Last().Split('.').First();

            return fileName;
        }

        /// <summary>
        /// try to convert a string to a enum, if not possible pass the default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string value, T defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                var result = (T) Enum.Parse(typeof(T), value);
                return result;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static float RoundTo(this float value, int digits)
        {
            return Convert.ToSingle(Math.Round(value, digits));
        }

        /// <summary>
        /// difference with a tolerance
        /// </summary>
        public static bool DifferenceCompare(this float value, float other, float tolerance)
        {
            var diff = Mathf.Abs(value - other);

            return (diff < tolerance);
        }

        public static T NextOf<T>(this IList<T> list, T item)
        {
            return list[(list.IndexOf(item) + 1) == list.Count ? 0 : (list.IndexOf(item) + 1)];
        }

        //Record Class defaults
        public static FieldInfo[] GetFilteredFieldInfos(this Type type, BindingFlags? bindingFlags)
        {
            var fieldInfos = bindingFlags == null
                ? type.GetFields().ToList()
                : type.GetFields((BindingFlags) bindingFlags).ToList();
            return fieldInfos.Where(x => x.GetCustomAttributes(typeof(SkipFieldAttribute), true).Length == 0).ToArray();
        }


        public static List<object> RecordClassDefault(this Object obj, BindingFlags? bindingFlags)
        {
            var type = obj.GetType();
            var fieldInfos = bindingFlags == null
                ? type.GetFilteredFieldInfos(null).ToList()
                : type.GetFilteredFieldInfos((BindingFlags) bindingFlags).ToList();

            List<object> fieldValues = new List<object>();
            foreach (var fieldInfo in fieldInfos)
            {
                fieldValues.Add(fieldInfo.GetValue(obj));
                //Debug.Log(fieldInfo.Name);
            }

            return fieldValues;
        }

        public static void SetClassDefaults(this Object obj, List<object> defaultFieldValues,
            BindingFlags? bindingFlags)
        {
            var type = obj.GetType();
            var fieldInfos = bindingFlags == null
                ? type.GetFilteredFieldInfos(null).ToList()
                : type.GetFilteredFieldInfos((BindingFlags) bindingFlags).ToList();

            for (int i = 0; i < fieldInfos.Count; ++i)
            {
                fieldInfos[i].SetValue(obj, defaultFieldValues[i]);
                //Debug.Log(fieldInfos[i].Name + "Value: " + defaultFieldValues[i]);
            }
        }

        //-- recording transform information
        public static List<Vector3> RecordPosition(this IList<UnityEngine.Object> unityObjects)
        {
            var positionList = new List<Vector3>();
            foreach (var obj in unityObjects)
            {
                if (obj is GameObject)
                {
                    positionList.Add(((GameObject) obj).transform.position);
                }
                else if (obj is Component)
                {
                    positionList.Add(((Component) obj).transform.position);
                }
                else
                {
                    throw new Exception("This object has to be a GameObject or a Component or a UI Component!");
                }
            }

            return positionList;
        }

        //method overloaded for Image elements as Image is derived from UnityEngine.Object
        public static List<Vector3> RecordPosition(this IList<Image> images)
        {
            var positionList = new List<Vector3>();
            foreach (var obj in images)
            {
                positionList.Add(obj.transform.position);
            }

            return positionList;
        }

        public static List<Vector2> RecordSizeDelta(this IList<UnityEngine.Object> unityObjects)
        {
            var sizeDeltaList = new List<Vector2>();
            foreach (var obj in unityObjects)
            {
                RectTransform rectTransform;
                if (obj is GameObject)
                {
                    rectTransform = ((GameObject) obj).GetComponent<RectTransform>();
                }
                else if (obj is Component)
                {
                    rectTransform = ((Component) obj).GetComponent<RectTransform>();
                }
                else
                {
                    throw new Exception("This object has to be a GameObject or a Component or a UI Component!");
                }

                if (rectTransform)
                    sizeDeltaList.Add(rectTransform.sizeDelta);
            }

            return sizeDeltaList;
        }

        public static List<Vector2> RecordSizeDelta(this IList<Image> images)
        {
            var sizeDeltaList = new List<Vector2>();
            foreach (var obj in images)
            {
                sizeDeltaList.Add(obj.rectTransform.sizeDelta);
            }

            return sizeDeltaList;
        }


        public static void SetPositions(this IEnumerable<UnityEngine.Object> unityObjects, IList<Vector3> positionList)
        {
            unityObjects.Each((x, i) =>
            {
                if (x is GameObject)
                {
                    ((GameObject) x).transform.position = positionList[i];
                }
                else if (x is Component)
                {
                    ((Component) x).transform.position = positionList[i];
                }
                else
                {
                    throw new Exception("This object has to be a GameObject or a Component or a UI Component!");
                }
            });
        }

        //method overloaded for Image elements as Image is derived from UnityEngine.Object
        public static void SetPositions(this IList<Image> images, IList<Vector3> positionList)
        {
            images.Each((x, i) => { x.transform.position = positionList[i]; });
        }

        public static void SetSizeDelta(this IList<UnityEngine.Object> unityObjects, IList<Vector2> sizeDeltaList)
        {
            unityObjects.Each((x, i) =>
            {
                RectTransform rectTransform;
                if (x is GameObject)
                {
                    rectTransform = ((GameObject) x).GetComponent<RectTransform>();
                }
                else if (x is Component)
                {
                    rectTransform = ((Component) x).GetComponent<RectTransform>();
                }
                else
                {
                    throw new Exception("This object has to be a GameObject or a Component or a UI Component!");
                }

                if (rectTransform)
                    rectTransform.sizeDelta = sizeDeltaList[i];
            });
        }

        public static void SetSizeDelta(this IList<Image> images, IList<Vector2> sizeDeltaList)
        {
            images.Each((x, i) => x.rectTransform.sizeDelta = sizeDeltaList[i]);
        }

        //----------

        public static string Right(this string value, int length)
        {
            return value.Substring(value.Length - length);
        }

        /// <summary>
        /// Parse Hex color code to Color
        /// </summary>
        public static Color ParseHtmlColor(this string value, Color? defaultColor = null)
        {
            Color returnColor;

            if (ColorUtility.TryParseHtmlString(value, out returnColor))
            {
                return returnColor;
            }

            return defaultColor ?? Color.white;
        }

        /// <summary>
        /// Retrieve android api level
        /// </summary>
        /// <returns></returns>
        public static int GetAndroidSdkInt()
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }


        /// <summary>
        /// checks if the named class exists in the assembly
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static bool CheckIfClassExist(string className)
        {
            Type classQuery;
            try
            {
                classQuery = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()
                    where type.Name == className
                    select type).First();
            }
            catch (Exception)
            {
                classQuery = null;
            }


            return classQuery != null;
        }
    }
}