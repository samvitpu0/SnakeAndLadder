using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    public class FixTransform : MonoBehaviour
    {
        [Header("Rotation")]
        public Quaternion DefaultRotation;

        public bool LockRotation;
        public bool UseDefaultRoation;

        [Header("Position")]
        public Vector3 DefaultLocalPosition;

        public bool LockPositionX, LockPositionY, LockPositionz;
        public bool UseDefaultPosition;
        public bool UseRootAsRelative;

        private Quaternion _worldRotation;
        private Vector3 _worldLocation;

        void OnEnable()
        {
            CaptureTransform();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (LockPositionX || LockPositionY || LockPositionz)
            {
                transform.position = new Vector3(LockPositionX ? _worldLocation.x : transform.position.x, LockPositionY ? _worldLocation.y : transform.position.y, LockPositionz ? _worldLocation.z : transform.position.z);
            }

            if (LockRotation)
                transform.rotation = _worldRotation;
        }

        void CaptureTransform()
        {
            _worldLocation = UseDefaultPosition ? (UseRootAsRelative ? transform.root : transform).TransformPoint(DefaultLocalPosition) : transform.position;

            _worldRotation = UseDefaultRoation ? DefaultRotation : transform.rotation;
        }
    }
}