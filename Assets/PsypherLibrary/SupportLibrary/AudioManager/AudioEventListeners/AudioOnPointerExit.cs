using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnPointerExit : AudioBase, IPointerExitHandler
    {
        public void OnPointerExit(PointerEventData eventData)
        {
            PlayAudio();
        }
    }
}