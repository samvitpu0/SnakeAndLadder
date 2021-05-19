using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnSubmit : AudioBase, ISubmitHandler
    {
        public void OnSubmit(BaseEventData eventData)
        {
            PlayAudio();
        }
    }
}