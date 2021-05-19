using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnCancel : AudioBase, ICancelHandler
    {
        public void OnCancel(BaseEventData eventData)
        {
            PlayAudio();
        }
    }
}