using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PsypherLibrary.SupportLibrary.Utils.FileManager
{
    [Serializable]
    public class DataStorage
    {
        public string DatabaseName;


        public string CollectionName;


        public List<JObject> JsonList = new List<JObject>();

        public DataStorage()
        {
            DatabaseName = "";
            CollectionName = "";
            JsonList = new List<JObject>();
        }
    }
}