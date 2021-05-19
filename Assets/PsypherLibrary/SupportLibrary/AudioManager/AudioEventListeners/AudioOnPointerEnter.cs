using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnPointerEnter : AudioBase, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}