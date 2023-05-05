using Custom.Pathfinding;
using UnityEditor;
using UnityEngine;

namespace CommonEditor.Pathfinding
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PF_Instance))]
    public class PF_InstanceEditor : Editor
    {
        private PF_Instance Script
            => (PF_Instance)target;

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
