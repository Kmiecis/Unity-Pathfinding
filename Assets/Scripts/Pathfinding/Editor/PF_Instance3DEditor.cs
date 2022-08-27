#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Custom.Pathfinding
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PF_Instance3D))]
    public class PF_Instance3DEditor : Editor
    {
        private PF_Instance3D Script
        {
            get => (PF_Instance3D)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();

            if (GUILayout.Button("Bake"))
            {
                Script.Bake();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
