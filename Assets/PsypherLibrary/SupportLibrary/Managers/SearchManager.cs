using System;
using System.Collections.Generic;
using System.Linq;

namespace PsypherLibrary.SupportLibrary.Managers
{
    [Serializable]
    public abstract class TaggedElement
    {
        public string Tags = string.Empty;
        public abstract object GetData();
    }

    public static class SearchManager
    {
        static Dictionary<string, List<object>> SearchData = new Dictionary<string, List<object>>();

        public static void SetData<T>(List<T> data, bool clearPreviousData = true) where T : TaggedElement
        {
            if (clearPreviousData) SearchData.Clear();

            foreach (T item in data)
            {
                var tags = item.Tags.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(x => x.ToUpper()).ToList();
                foreach (var tag in tags)
                {
                    if (SearchData.ContainsKey(tag))
                        SearchData[tag].Add(item);
                    else
                    {
                        SearchData.Add(tag, new List<object> {item.GetData()});
                    }
                }
            }
        }

        public static List<object> GetItemForTag(string input)
        {
            input = input.ToUpper();

            var data = new List<object>();
            if (input.Length < 3 || SearchData.Count == 0)
                return data;

            foreach (var kvp in SearchData)
            {
                if (kvp.Key.StartsWith(input))
                    data.AddRange(kvp.Value);
            }

            return data;
        }
    }
}