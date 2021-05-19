namespace PsypherLibrary.SupportLibrary.UIHelperScripts
{
    public class UIPrefabUnit<T> : UIPanel
    {
        public T mData;

        public virtual void UpdateUI()
        {
        }

        public virtual void SetData(T data)
        {
            mData = data;
            UpdateUI();
        }

        public virtual void FlushUI()
        {
        }
    }
}