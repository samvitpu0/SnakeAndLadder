using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;

#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary.CategorySystem
{
    public class ThumbnailHybrid : MonoBehaviour
    {
        private ThumbnailHelper _thumbnailHelper;

        public ThumbnailHelper ThumbnailHelper
        {
            get { return _thumbnailHelper; }
        }
        public GameObject CategoryThumbnail;
        public GameObject ItemThumbnail;


        void Awake()
        {
            CategoryThumbnail.Deactivate();
            ItemThumbnail.Deactivate();
        }

        public ThumbnailHelper SetThumbnailType(ThumbnailTypes type)
        {
            switch (type)
            {
                case ThumbnailTypes.Category:
                {
                    CategoryThumbnail.Activate();
                    _thumbnailHelper = CategoryThumbnail.GetComponent<ThumbnailHelper>();
                }
                    break;
                case ThumbnailTypes.Item:
                {
                    ItemThumbnail.Activate();
                    _thumbnailHelper = ItemThumbnail.GetComponent<ThumbnailHelper>();
                }
                    break;
            }

            return ThumbnailHelper;
        }
    }
}
#endif