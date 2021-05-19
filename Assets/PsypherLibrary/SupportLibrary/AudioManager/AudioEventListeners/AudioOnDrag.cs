using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnDrag : AudioBase, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}