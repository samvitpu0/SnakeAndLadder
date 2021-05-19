using System;
using PsypherLibrary.SupportLibrary.Utils.FileManager;

namespace PsypherLibrary.SupportLibrary.BaseInterfaces
{
    public interface ILoginService
    {
        void Login(Action<DataStorage> OnSuccess, Action<Exception> OnFailure);
    }
}