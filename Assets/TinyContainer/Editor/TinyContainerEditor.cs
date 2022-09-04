using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Jnk.TinyContainer.Editor
{
    [CustomEditor(typeof(TinyContainer))]
    public class TinyContainerEditor : UnityEditor.Editor
    {
        private SerializedProperty _enabledEventFunctionsProp;
        private SerializedProperty _disposeOnDestroyProp;

        private void OnEnable()
        {
            _enabledEventFunctionsProp = serializedObject.FindProperty("enabledEventFunctions");
            _disposeOnDestroyProp = serializedObject.FindProperty("disposeOnDestroy");
        }

        public override void OnInspectorGUI()
        {
            var container = target as TinyContainer;

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(_enabledEventFunctionsProp);
            EditorGUILayout.PropertyField(_disposeOnDestroyProp, new GUIContent("Dispose IDisposables On Destroy"));

            var instances = typeof(TinyContainer)
                .GetField("_instances", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(container) as Dictionary<Type, object>;

            if (instances?.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Registered Instances", EditorStyles.boldLabel);

                foreach (KeyValuePair<Type, object> pair in instances)
                    EditorGUILayout.LabelField(pair.Key.FullName);
            }

            var factories = typeof(TinyContainer)
                .GetField("_factories", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(container) as Dictionary<Type, Func<TinyContainer, object>>;

            if (factories?.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Registered Factories", EditorStyles.boldLabel);

                foreach (KeyValuePair<Type, Func<TinyContainer, object>> pair in factories)
                    EditorGUILayout.LabelField(pair.Key.FullName);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
