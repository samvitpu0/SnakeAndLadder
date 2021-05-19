using UnityEditor;
#if UNITY_EDITOR
using PsypherLibrary.SupportLibrary.Utils.UI;

namespace PsypherLibrary.SupportLibrary.Editor.UI
{
    [CustomEditor(typeof(TransformChangedEvent))]
    public class TransformChangedEditor : PropertyEditorSupport
    {
        //private TransformChanged _binder;
        private bool _enableExtremeIndexes;
        private bool _enableUnityEvent;

        private SerializedProperty
            _siblingIndex,
            _extremeSibling,
            _onTransformChanged,
            _enableEvent,
            _useExtremeIndex;


        protected override void Initialize()
        {
            // _binder = target as TransformChanged;

            _siblingIndex = serializedObject.FindProperty("SiblingIndex");
            _extremeSibling = serializedObject.FindProperty("ExtremeSibling");
            _onTransformChanged = serializedObject.FindProperty("OnTransformChanged");
            _enableEvent = serializedObject.FindProperty("EnableEvent");
            _useExtremeIndex = serializedObject.FindProperty("UseExtremeIndex");

            //init values from the actual class
            _enableUnityEvent = _enableEvent.boolValue;
            _enableExtremeIndexes = _useExtremeIndex.boolValue;
        }

        public override void OnInspectorGUI()
        {
            BeginEdit();
            BeginSection("Sibling Properties");

            _enableExtremeIndexes = EditorGUILayout.BeginToggleGroup("Extreme Index", _enableExtremeIndexes);
            PropertyField("Select Extreme", _extremeSibling);
            EditorGUILayout.EndToggleGroup();

            _useExtremeIndex.boolValue = _enableExtremeIndexes;
            if (!_enableExtremeIndexes)
            {
                PropertyField("Sibling Index", _siblingIndex);
            }

            EndSection();

            BeginSection("Events");

            _enableUnityEvent = EditorGUILayout.Toggle("Enable Event", _enableUnityEvent);
            _enableEvent.boolValue = _enableUnityEvent;

            if (_enableUnityEvent)
            {
                PropertyField("On Transform Values Change", _onTransformChanged);
            }

            EndSection();

            EndEdit();
        }
    }
}

#endif