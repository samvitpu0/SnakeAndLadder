using System.Collections.Generic;
using System.Linq;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary._ExampleScripts
{
    public class UIScrollableExample : MonoBehaviour
    {
        public GameObject prefab;

        public UIScrollable scroll;

        //Data
        List<string> alphabets = Enumerable.Range(1, 1500).Select(x => x.ToString()).ToList();

        void Start()
        {
            scroll.SetPrefab(prefab)
                .SetData(alphabets).SetFunction(
                    (data, index, obj) =>
                    {
                        var d = ((List<string>) data);
                        obj.GetComponentInChildren<Text>().SetText(d[index]);
                    }).Initialize();
        }
    }
}