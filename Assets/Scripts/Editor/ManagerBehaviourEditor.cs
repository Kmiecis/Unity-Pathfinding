using UnityEditor;
using UnityEngine;

namespace Custom
{
    [CustomEditor(typeof(ManagerBehaviour))]
    public class ManagerBehaviourEditor : Editor
    {
        private ManagerBehaviour Script => (ManagerBehaviour)target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Find unit path"))
            {
                Script.SetUnitPath();
            }
        }
    }
}
