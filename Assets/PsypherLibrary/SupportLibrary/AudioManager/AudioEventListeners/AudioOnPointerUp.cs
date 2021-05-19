using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnPointerUp : AudioBase, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}