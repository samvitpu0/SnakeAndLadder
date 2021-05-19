using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.Generics
{
    public class GenericManager<T> : GenericSingleton<T> where T : MonoBehaviour
    {
        protected override bool IsPersistent => true;
    }
}