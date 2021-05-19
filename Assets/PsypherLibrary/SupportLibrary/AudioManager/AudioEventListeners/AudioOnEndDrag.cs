using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnEndDrag : AudioBase, IEndDragHandler
    {
        public void OnEndDrag(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}