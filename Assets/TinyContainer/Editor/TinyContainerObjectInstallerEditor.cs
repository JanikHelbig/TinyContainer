using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Jnk.TinyContainer.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TinyContainerObjectInstaller))]
    public class TinyContainerObjectInstallerEditor : UnityEditor.Editor
    {
        private SerializedProperty _objectsProp;
        private  readonly StringBuilder _stringBuilder = new();

        private void OnEnable()
        {
            _objectsProp = serializedObject.FindProperty("objects");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(_objectsProp);

            serializedObject.ApplyModifiedProperties();

            using var pooledList = ListPool<Object>.Get(out var suspiciousObjects);

            for (var i = 0; i < _objectsProp.arraySize; i++)
            {
                Object objectValue = _objectsProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (objectValue is GameObject or Material or Transform)
                    suspiciousObjects.Add(objectValue);
            }

            if (suspiciousObjects.Count <= 0)
                return;

            _stringBuilder.Clear();
            _stringBuilder.Append("Are you sure you want the following types to be registered with these objects?");

            foreach (Object obj in suspiciousObjects)
                _stringBuilder.AppendLine().Append("• ").Append(obj.GetType().FullName).Append(" (").Append(obj.name).Append(")");

            EditorGUILayout.HelpBox(_stringBuilder.ToString(), MessageType.Warning);
        }
    }
}
