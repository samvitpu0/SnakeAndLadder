using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnBeginDrag : AudioBase, IBeginDragHandler
    {
        public void OnBeginDrag(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}