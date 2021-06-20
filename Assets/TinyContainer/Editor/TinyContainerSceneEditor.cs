using UnityEditor;
using System.Linq;
using UnityEngine;

namespace Jnk.TinyContainer.Editor
{
    [CustomEditor(typeof(TinyContainerScene))]
    public class TinyContainerSceneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var sceneContainer = (TinyContainerScene) target;

            var isOnlyOneInScene = true;

            foreach (GameObject gameObject in sceneContainer.gameObject.scene.GetRootGameObjects())
            {
                if (gameObject.TryGetComponent(out TinyContainerScene container) && container != sceneContainer)
                    isOnlyOneInScene = false;
            }

            if (isOnlyOneInScene)
            {
                EditorGUILayout.HelpBox("This container will be configured for this scene.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox($"More than one {nameof(TinyContainerScene)} found in the scene.\nMake sure only one TinyContainer in the scene has a {nameof(TinyContainerScene)} component.", MessageType.Warning);
            }
        }
    }
}
