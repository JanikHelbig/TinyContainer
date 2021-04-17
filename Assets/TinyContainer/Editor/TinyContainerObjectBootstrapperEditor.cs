using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Jnk.TinyContainer.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TinyContainerObjectBootstrapper))]
    public class TinyContainerObjectBootstrapperEditor : UnityEditor.Editor
    {
        private SerializedProperty _objectsProp;
        private  readonly StringBuilder _stringBuilder = new StringBuilder();

        private void OnEnable()
        {
            _objectsProp = serializedObject.FindProperty("objects");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(_objectsProp);

            var objects = typeof(TinyContainerObjectBootstrapper)
                .GetField("objects", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(target as TinyContainerObjectBootstrapper) as List<Object>;

            if (objects?.Count > 0)
            {
                List<Object> suspiciousTypes = objects
                    .Where(x => x is GameObject || x is Material || x is Transform)
                    .ToList();

                if (suspiciousTypes.Count == 0)
                    return;

                _stringBuilder.Clear();
                _stringBuilder.Append("Are you sure you want the following types to be registered with these objects?");

                foreach (Object type in suspiciousTypes)
                    _stringBuilder.AppendLine().Append("• ").Append(type.GetType().FullName).Append(" (").Append(type.name).Append(")");

                EditorGUILayout.HelpBox(_stringBuilder.ToString(), MessageType.Warning);
            }

            if (GUI.changed)
                serializedObject.ApplyModifiedProperties();
        }
    }
}
