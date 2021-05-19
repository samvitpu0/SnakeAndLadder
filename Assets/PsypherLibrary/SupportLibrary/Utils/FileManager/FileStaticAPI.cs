using System.IO;
using UnityEngine;
using PsypherLibrary.SupportLibrary.Extensions;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace PsypherLibrary.SupportLibrary.Utils.FileManager
{
    public static class FileStaticAPI
    {
        #region in game data path

        public static bool IsFileExists(string fileName)
        {
            if (fileName.Equals(string.Empty)) return false;

            return File.Exists(GetFullPath(fileName));
        }

        public static void CreateFile(string fileName)
        {
            if (!IsFileExists(fileName))
            {
                CreateFolder(fileName.Substring(0, fileName.LastIndexOf('/')));

                File.Create(GetFullPath(fileName));
            }
        }

        public static void Write(string fileName, string contents)
        {
            CreateFolder(fileName.Substring(0, fileName.LastIndexOf('/')));

            TextWriter tw = new StreamWriter(GetFullPath(fileName), false);
            tw.Write(contents);
            tw.Close();

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

            //File.WriteAllText(GetFullPath (fileName), contents);
        }

        public static void Write(string filename, byte[] byteArray)
        {
            CreateFolder(filename.Substring(0, filename.LastIndexOf('/')));

            File.WriteAllBytes(GetFullPath(filename), byteArray);


#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public static string Read(string fileName)
        {
#if !UNITY_WEBPLAYER
            if (IsFileExists(fileName))
                return File.ReadAllText(GetFullPath(fileName));
            return "";
#endif

#if UNITY_WEBPLAYER
		Debug.LogWarning("FileStaticAPI::Read is ignored under wep player platfrom");
		return "";
#endif
        }

        public static void CopyFile(string srcFileName, string destFileName)
        {
            if (IsFileExists(srcFileName) && !srcFileName.Equals(destFileName))
            {
                var index = destFileName.LastIndexOf("/");
                var filePath = string.Empty;

                if (index != -1) filePath = destFileName.Substring(0, index);

                if (!Directory.Exists(GetFullPath(filePath))) Directory.CreateDirectory(GetFullPath(filePath));

                File.Copy(GetFullPath(srcFileName), GetFullPath(destFileName), true);

#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
        }

        public static void DeleteFile(string fileName, bool refresh = true)
        {
            if (IsFileExists(fileName))
            {
                File.Delete(GetFullPath(fileName));

#if UNITY_EDITOR
                if (refresh) AssetDatabase.Refresh();
#endif
            }
        }

        public static bool IsFolderExists(string folderPath)
        {
            if (folderPath.Equals(string.Empty)) return false;

            return Directory.Exists(GetFullPath(folderPath));
        }

        public static void CreateFolder(string folderPath)
        {
            if (!IsFolderExists(folderPath))
            {
                Directory.CreateDirectory(GetFullPath(folderPath));

#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
        }

        public static void CopyFolder(string srcFolderPath, string destFolderPath)
        {
#if !UNITY_WEBPLAYER
            if (!IsFolderExists(srcFolderPath)) return;

            CreateFolder(destFolderPath);


            srcFolderPath = GetFullPath(srcFolderPath);
            destFolderPath = GetFullPath(destFolderPath);

            //Now Create all of the directories
            foreach (var dirPath in Directory.GetDirectories(srcFolderPath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(srcFolderPath, destFolderPath));

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(srcFolderPath, "*.*", SearchOption.AllDirectories)) File.Copy(newPath, newPath.Replace(srcFolderPath, destFolderPath), true);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
#endif

#if UNITY_WEBPLAYER
		Debug.LogWarning("FileStaticAPI::CopyFolder is innored under wep player platfrom");
#endif
        }

        public static void DeleteFolder(string folderPath, bool refresh = true)
        {
#if !UNITY_WEBPLAYER
            if (IsFolderExists(folderPath))
            {
                Directory.Delete(GetFullPath(folderPath), true);

#if UNITY_EDITOR
                if (refresh) AssetDatabase.Refresh();
#endif
            }
#endif

#if UNITY_WEBPLAYER
		Debug.LogWarning("FileStaticAPI::DeleteFolder is innored under wep player platfrom");
#endif
        }

        private static string GetFullPath(string srcName)
        {
            if (srcName.Equals(string.Empty)) return Application.dataPath;

            if (srcName[0].Equals('/')) srcName.Remove(0, 1);

            return Application.dataPath + "/" + srcName;
        }


#if UNITY_EDITOR
        //////////////////////////////////////////////////////////////////////
        /// UPDATE WITH AssetDatabase interface
        //////////////////////////////////////////////////////////////////////	
        public static void CreateAssetFolder(string assetFolderPath)
        {
            if (!IsFolderExists(assetFolderPath))
            {
                var index = assetFolderPath.IndexOf("/");
                var offset = 0;
                var parentFolder = "Assets";
                while (index != -1)
                {
                    if (!Directory.Exists(GetFullPath(assetFolderPath.Substring(0, index))))
                    {
                        var guid = AssetDatabase.CreateFolder(parentFolder, assetFolderPath.Substring(offset, index - offset));
                        AssetDatabase.GUIDToAssetPath(guid);
                    }

                    offset = index + 1;
                    parentFolder = "Assets/" + assetFolderPath.Substring(0, offset - 1);
                    index = assetFolderPath.IndexOf("/", index + 1);
                }

                AssetDatabase.Refresh();
            }
        }

        public static void CopyAsset(string srcAssetName, string destAssetName)
        {
            if (IsFileExists(srcAssetName) && !srcAssetName.Equals(destAssetName))
            {
                var index = destAssetName.LastIndexOf("/");
                var filePath = string.Empty;

                if (index != -1)
                {
                    filePath = destAssetName.Substring(0, index + 1);
                    //Create asset folder if needed
                    CreateAssetFolder(filePath);
                }


                AssetDatabase.CopyAsset(GetFullAssetPath(srcAssetName), GetFullAssetPath(destAssetName));
                AssetDatabase.Refresh();
            }
        }

        public static void DeleteAsset(string assetName)
        {
            if (IsFileExists(assetName))
            {
                AssetDatabase.DeleteAsset(GetFullAssetPath(assetName));
                AssetDatabase.Refresh();
            }
        }

        private static string GetFullAssetPath(string assetName)
        {
            if (assetName.Equals(string.Empty)) return "Assets/";

            if (assetName[0].Equals('/')) assetName.Remove(0, 1);

            return "Assets/" + assetName;
        }

#endif
        //////////////////////////////////////////////////////////////////////

        #endregion

        #region in persistant data path

        private static string GetFullPathInPersistant(string srcName)
        {
            if (srcName.Equals(string.Empty)) return PiUtilities.SavePath;

            if (srcName[0].Equals('/')) srcName.Remove(0, 1);

            return PiUtilities.SavePath + "/" + srcName;
        }

        public static bool IsFileExistsInPersistant(string fileName)
        {
            if (fileName.Equals(string.Empty)) return false;

            return File.Exists(GetFullPathInPersistant(fileName));
        }

        public static void CreateFileInPersistant(string fileName)
        {
            if (!IsFileExistsInPersistant(fileName))
            {
                CreateFolderInPersistant(fileName.Substring(0, fileName.LastIndexOf('/')));

                File.Create(GetFullPathInPersistant(fileName));
            }
        }

        public static void WriteInPersistant(string fullPath, string contents)
        {
            TextWriter tw = new StreamWriter(fullPath, false);
            tw.Write(contents);
            tw.Close();
        }

        public static void WriteInPersistant(string fullPath, byte[] byteArray)
        {
            using (var file = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var binary = new BinaryWriter(file))
            {
                binary.Write(byteArray);
                binary.Close();
            }
        }

        /// <summary>
        /// Create a increment filename and returns the path
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string CreateFileIncrementInPersistant(string filename, string extension)
        {
            var fileCount = 0;

            CreateFolderInPersistant(filename.Substring(0, filename.LastIndexOf('/'))); //create the specific folder if not existing already

            while (IsFileExistsInPersistant(filename + (fileCount > 0 ? "_" + (fileCount) : string.Empty) + "." + extension))
            {
                fileCount++;
            }

            var fs = File.Create(GetFullPathInPersistant(filename + (fileCount > 0 ? "_" + fileCount : "") + "." + extension));

            fs.Close();

            return fs.Name;
        }

        public static bool IsFolderExistsInPersistant(string folderPath)
        {
            if (folderPath.Equals(string.Empty)) return false;

            return Directory.Exists(GetFullPathInPersistant(folderPath));
        }

        public static void CreateFolderInPersistant(string folderPath)
        {
            if (!IsFolderExistsInPersistant(folderPath))
            {
                Directory.CreateDirectory(GetFullPathInPersistant(folderPath));
            }
        }

        public static string ReadInPersistant(string fileName)
        {
#if !UNITY_WEBPLAYER
            if (IsFileExistsInPersistant(fileName))
                return File.ReadAllText(GetFullPathInPersistant(fileName));
            return "";
#endif

#if UNITY_WEBPLAYER
		Debug.LogWarning("FileStaticAPI::Read is ignored under wep player platfrom");
		return "";
#endif
        }

        public static void CopyFileInPersistant(string srcFileName, string destFileName)
        {
            if (IsFileExistsInPersistant(srcFileName) && !srcFileName.Equals(destFileName))
            {
                var index = destFileName.LastIndexOf("/");
                var filePath = string.Empty;

                if (index != -1) filePath = destFileName.Substring(0, index);

                if (!Directory.Exists(GetFullPathInPersistant(filePath))) Directory.CreateDirectory(GetFullPath(filePath));

                File.Copy(GetFullPathInPersistant(srcFileName), GetFullPathInPersistant(destFileName), true);
            }
        }

        public static void DeleteFileInPersistant(string fileName, bool refresh = true)
        {
            if (IsFileExistsInPersistant(fileName))
            {
                File.Delete(GetFullPathInPersistant(fileName));
            }
        }

        #endregion
    }
}