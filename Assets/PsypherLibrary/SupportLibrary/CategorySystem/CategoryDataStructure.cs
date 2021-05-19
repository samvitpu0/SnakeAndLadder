using System;
using Newtonsoft.Json.Linq;
using PsypherLibrary.SupportLibrary.Managers;
using UnityEngine;


#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary.CategorySystem
{
    #region Enums

    public enum ThumbnailTypes
    {
        Category,
        Item,
        ItemMinimal
    }

    #endregion

    #region Data Structure

    [Serializable]
    public class CategoryCollection
    {
        //public List<Category> Collections = new List<Category>();
        public JArray Collections = new JArray();
    }

    [Serializable]
    public class Category
    {
        public string Name;
        public string Thumbnail;

        [SerializeField]
        public JArray CategoryData = new JArray();

        public Category(string name, string thumbnail, JArray data)
        {
            Name = name;
            Thumbnail = thumbnail;
            CategoryData = data;
        }
    }

    [Serializable]
    public class Item : TaggedElement
    {
        public string Name;
        public string Thumbnail;
        public bool IsFree;
        public string UploadDate;
        public string ContentType;
        public string UID;
        public int PositionInList;
        public JObject Data;

        public Item(string name, string thumbnail, JObject data, string uploadDate, string contentType, string uid, bool isFree, int position)
        {
            Name = name;
            Thumbnail = thumbnail;
            IsFree = isFree;
            UploadDate = uploadDate;
            ContentType = contentType;
            UID = uid;
            PositionInList = position;
            Data = data;
        }

        public Item()
        {
        }

        public override object GetData()
        {
            return this;
        }
    }

    #endregion

    #region Interfaces

    public interface ICategoryClickable
    {
        void OnClick(CategorySystemInfo categoryInfo, ThumbnailHelper clickedOn);
    }

    #endregion
}
#endif