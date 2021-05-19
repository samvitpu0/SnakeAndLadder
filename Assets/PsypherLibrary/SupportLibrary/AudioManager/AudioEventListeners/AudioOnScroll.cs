using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnScroll : AudioBase, IScrollHandler
    {
        public void OnScroll(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}