using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnSelect : AudioBase, ISelectHandler
    {
        public void OnSelect(BaseEventData eventData)
        {
            PlayAudio();
        }
    }
}