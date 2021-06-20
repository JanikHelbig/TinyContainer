using System;
using UnityEditor;

namespace Jnk.TinyContainer.Editor
{
    [CustomEditor(typeof(TinyContainerRoot))]
    public class TinyContainerRootEditor : UnityEditor.Editor
    {
        private SerializedProperty _dontDestroyOnLoadProp;

        private void OnEnable()
        {
            _dontDestroyOnLoadProp = serializedObject.FindProperty("dontDestroyOnLoad");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            bool isOnlyOneInScene = FindObjectsOfType<TinyContainerRoot>().Length <= 1;
            if (isOnlyOneInScene)
            {
                EditorGUILayout.HelpBox("This container will be the Root of the container hierarchy.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox($"More than one {nameof(TinyContainerRoot)} found in the scene.\nMake sure only one TinyContainer has a {nameof(TinyContainerRoot)} component.", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(_dontDestroyOnLoadProp);


            serializedObject.ApplyModifiedProperties();
        }
    }
}
