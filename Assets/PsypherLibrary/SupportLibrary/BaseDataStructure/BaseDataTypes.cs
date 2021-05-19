using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PsypherLibrary.SupportLibrary.Extensions;
using Random = UnityEngine.Random;

namespace PsypherLibrary.SupportLibrary.BaseDataStructure
{
    #region Base Enums

    public enum EService
    {
        None = 0,
        GameSparks = 1
    }

    [Serializable]
    public struct EEndPoints
    {
        public string VersionEndPoint;
        public String ConfigEndPoint;
    }

    public enum ELoginType
    {
        Device,
        Facebook
    }

    public enum EScenes
    {
        [Description("Splash")]
        Splash,

        [Description("MainMenu")]
        MainMenu,

        [Description("Store")]
        Store,

        [Description("Game")]
        Game,

        [Description("Tutorial")]
        Tutorial,

        [Description("Map")]
        Map,

        [Description("TutorialMap")]
        MapTutorial,
    }

    public enum EDragDirections
    {
        Right,
        Left,
        Up,
        Down
    }

    /// <summary>
    /// percentage based size enums
    /// </summary>
    public enum EPanelSize
    {
        [Description("75|80")]
        SmallPortait,

        [Description("85|85")]
        MediumPortrait,

        [Description("95|95")]
        LargePortrait,

        [Description("50|80")]
        SmallLandscape,

        [Description("60|90")]
        MediumLandscape,

        [Description("70|95")]
        LargeLandscape,
    }

    #endregion

    #region Base DataStructures

    [Serializable]
    public class KeyValue<T, U>
    {
        public T Key;
        public U Value;

        public KeyValue()
        {
        }

        public KeyValue(T key, U value)
        {
            Key = key;
            Value = value;
        }
    }

    public class CoreData
    {
        public string Name;
        public string UID;

        public virtual string GetUniqueSortKey()
        {
            return UID.GetMD5CheckSum();
        }

        protected CoreData(string name, string uid)
        {
            Name = name;
            UID = uid;
        }

        protected CoreData()
        {
        }
    }

    /// <summary>
    /// custom 2D array to hold the cell value
    /// </summary>
    public class ObjectArray2D
    {
        private object[,] _objects;

        public object this[int row, int column]
        {
            get
            {
                try
                {
                    return _objects[row, column];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            }
            set => _objects[row, column] = value;
        }

        public ObjectArray2D(int sizeX, int sizeY)
        {
            _objects = new object[sizeX, sizeY];
        }
    }

    [Serializable]
    public class RangeData
    {
        public float Min, Max;

        public float RandomBetweenRangeInFloat()
        {
            return Random.Range(Min, Max);
        }

        public int RandomBetweenRangeInInt()
        {
            return (int) Random.Range(Min, Max);
        }
    }

    #region Game Configuration

    [Serializable]
    public class AppConfigData
    {
        //this empty class is need to get the main config json from server
        //todo: make localDataManager to fetch MainConfig without this class
    }

    #endregion

    #region Localization

    [Serializable]
    public class LocalizationData
    {
        public string[] Languages = new string[] { };
        public Dictionary<string, string[]> LocalizationTextBindings = new Dictionary<string, string[]>();
        public Dictionary<string, string[]> LocalizationAudioBindings = new Dictionary<string, string[]>();

        public void AddLanguage(string lang)
        {
            var existingLanguages = Languages.ToList();
            Languages = new string[Languages.Length + 1];
            for (int i = 0; i < existingLanguages.Count; i++)
            {
                Languages[i] = existingLanguages[i];
            }

            Languages[Languages.Length - 1] = lang;
            Console.WriteLine("Localized Languages: " + Languages.Length);

            var LocalizedTextTags = LocalizationTextBindings.Keys.ToList();
            foreach (var tag in LocalizedTextTags)
            {
                var existingBindings = LocalizationTextBindings[tag].ToList();
                LocalizationTextBindings[tag] = new string[Languages.Length];
                for (int i = 0; i < existingBindings.Count; i++)
                {
                    LocalizationTextBindings[tag][i] = existingBindings[i];
                }
            }

            var LocalizedAudioTags = LocalizationAudioBindings.Keys.ToList();
            foreach (var tag in LocalizedAudioTags)
            {
                var existingBindings = LocalizationAudioBindings[tag].ToList();
                LocalizationAudioBindings[tag] = new string[Languages.Length];
                for (int i = 0; i < existingBindings.Count; i++)
                {
                    LocalizationAudioBindings[tag][i] = existingBindings[i];
                }
            }
        }

        public void RemoveLanguage(string lang)
        {
            var list = Languages.ToList();
            var index = list.FindIndex(x => string.Equals(x, lang));
            list.RemoveAt(index);
            Languages = list.ToArray();
            var keys = LocalizationTextBindings.Keys.ToList();
            foreach (var key in keys)
            {
                var existingBindings = LocalizationTextBindings[key].ToList();
                existingBindings.RemoveAt(index);
                LocalizationTextBindings[key] = existingBindings.ToArray();
            }

            keys = LocalizationAudioBindings.Keys.ToList();
            foreach (var key in keys)
            {
                var existingBindings = LocalizationAudioBindings[key].ToList();
                existingBindings.RemoveAt(index);
                LocalizationAudioBindings[key] = existingBindings.ToArray();
            }
        }

        public void AddTextBinding(string language, string tag, string binding)
        {
            var index = Languages.ToList().FindIndex(x => string.Equals(x, language));

            if (!LocalizationTextBindings.ContainsKey(tag))
            {
                LocalizationTextBindings.Add(tag, new string[Languages.Length]);
            }

            LocalizationTextBindings[tag][index] = binding;
        }

        public void AddAudioBinding(string language, string tag, string binding)
        {
            var index = Languages.ToList().FindIndex(x => string.Equals(x, language));

            if (!LocalizationAudioBindings.ContainsKey(tag))
            {
                LocalizationAudioBindings.Add(tag, new string[Languages.Length]);
            }

            LocalizationAudioBindings[tag][index] = binding;
        }
    }

    #endregion

    #endregion
}