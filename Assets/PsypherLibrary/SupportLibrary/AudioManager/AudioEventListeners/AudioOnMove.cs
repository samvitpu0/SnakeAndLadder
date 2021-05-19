using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.AudioManager.AudioEventListeners
{
    public class AudioOnMove : AudioBase, IMoveHandler
    {
        public void OnMove(AxisEventData eventData)
        {
            PlayAudio();
        }
    }
}