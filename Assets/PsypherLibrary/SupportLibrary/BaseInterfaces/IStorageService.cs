using System;
using PsypherLibrary.SupportLibrary.Utils.FileManager;

namespace PsypherLibrary.SupportLibrary.BaseInterfaces
{
    public interface IStorageService
    {
        void GetConfigVersion(Action<DataStorage> OnSuccess, Action<Exception> OnFailure);

        void GetConfigFile(Action<DataStorage> OnSuccess, Action<Exception> OnFailure, string url);
    }
}