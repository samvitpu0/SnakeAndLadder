using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnPointerClick : AudioBase, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}