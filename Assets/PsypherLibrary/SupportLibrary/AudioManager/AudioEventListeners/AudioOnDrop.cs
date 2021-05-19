using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnDrop : AudioBase, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}