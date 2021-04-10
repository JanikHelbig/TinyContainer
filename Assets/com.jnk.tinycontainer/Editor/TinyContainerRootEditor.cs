using UnityEditor;

namespace Jnk.TinyContainer.Editor
{
    [CustomEditor(typeof(TinyContainerRoot))]
    public class TinyContainerRootEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var container = target as TinyContainer;
            
            EditorGUILayout.HelpBox("This container will be the root of the container hierarchy.", MessageType.Info);
        }
    }
}