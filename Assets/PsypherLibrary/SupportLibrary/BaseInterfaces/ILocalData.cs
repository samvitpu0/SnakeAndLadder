namespace PsypherLibrary.SupportLibrary.BaseInterfaces
{
    public interface ILocalData<T>
    {
        void Save();

        T Load();

        T Create();

        bool IsSaveAvailable();
    }
}