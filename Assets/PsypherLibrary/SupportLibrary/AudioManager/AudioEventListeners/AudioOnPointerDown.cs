using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnPointerDown : AudioBase, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}