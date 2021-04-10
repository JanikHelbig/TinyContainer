using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Jnk.TinyContainer.Editor
{
    [CustomEditor(typeof(TinyContainer))]
    public class TinyContainerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var container = target as TinyContainer;
            
            EditorGUILayout.HelpBox("This is a simple dependency container.", MessageType.Info);
            
            var instanceDictionary = typeof(TinyContainer)
                .GetField("_instanceDictionary", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(container) as Dictionary<Type, object>;
            
            if (instanceDictionary?.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Instances", EditorStyles.boldLabel);
                
                foreach (KeyValuePair<Type, object> pair in instanceDictionary)
                    EditorGUILayout.LabelField(pair.Key.FullName);
            }
            
            var factoryDictionary = typeof(TinyContainer)
                .GetField("_factoryDictionary", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(container) as Dictionary<Type, Func<TinyContainer, object>>;
            
            if (factoryDictionary?.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Factories", EditorStyles.boldLabel);
                
                foreach (KeyValuePair<Type, Func<TinyContainer, object>> pair in factoryDictionary)
                    EditorGUILayout.LabelField(pair.Key.FullName);
            }
        }
    }
}