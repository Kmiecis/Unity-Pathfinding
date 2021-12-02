using UnityEditor;
using UnityEngine;

namespace Custom
{
    [CustomEditor(typeof(UnitBehaviour))]
    public class UnitBehaviourEditor : Editor
    {
        private UnitBehaviour Script => (UnitBehaviour)target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Start follow path"))
            {
                Script.StartFollowPath();
            }
            if (GUILayout.Button("Stop follow path"))
            {
                Script.StopFollowPath();
            }
        }
    }
}
