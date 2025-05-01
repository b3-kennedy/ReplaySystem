// Put this in an Editor folder (e.g., "Editor/ReplayManagerEditor.cs")
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ReplayManager))]
public class ReplayManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ReplayManager manager = (ReplayManager)target;

        GUILayout.Label("Recorded Actions:", EditorStyles.boldLabel);

        foreach (var action in manager.actions)
        {
            if (action == null) continue;

            GUILayout.BeginVertical("box");
            GUILayout.Label($"Type: {action.GetType().Name}");
            GUILayout.Label($"Time: {action.timeStamp}");

            // Show extra info if it's a MovementAction
            if (action is MovementAction move)
            {
                GUILayout.Label($"Position: {move.targetPosition.ToVector3()}");
            }

            GUILayout.EndVertical();
        }
    }
}