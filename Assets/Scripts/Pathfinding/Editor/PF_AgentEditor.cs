#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Custom.Pathfinding
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PF_Agent))]
    public class PF_AgentEditor : Editor
    {
        private PF_Agent Script => (PF_Agent)target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Resume"))
            {
                Script.Resume();
            }
            if (GUILayout.Button("Halt"))
            {
                Script.Halt();
            }
        }
    }
}
#endif
