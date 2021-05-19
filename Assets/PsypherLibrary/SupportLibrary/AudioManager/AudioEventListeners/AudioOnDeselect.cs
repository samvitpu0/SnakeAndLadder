using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnDeselect : AudioBase, IDeselectHandler
    {
        public void OnDeselect(BaseEventData eventData)
        {
            PlayAudio();
        }
    }
}