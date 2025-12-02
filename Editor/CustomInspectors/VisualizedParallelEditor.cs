using BehaviourGraph.Visualizer;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph.Editor
{
    [CustomEditor(typeof(VisualizedParallel))]
    public class VisualizedParallelEditor : UnityEditor.Editor
    {
        private SerializedProperty mainStateProp;
        private SerializedProperty parallelStateProp;

        private void OnEnable()
        {
            mainStateProp = serializedObject.FindProperty("mainState");
            parallelStateProp = serializedObject.FindProperty("parallelState");
        }

        public override void OnInspectorGUI()
        {
            var vBranch = target as VisualizedParallel;
            if (vBranch == null)
                return;

            vBranch.FriendlyName = EditorGUILayout.TextField(new GUIContent("Friendly Name"), vBranch.FriendlyName);

            EditorGUILayout.Space();
            vBranch.Tag = EditorGUILayout.TextField(new GUIContent("Tag"), vBranch.Tag);

            Separator("Content");
            EditorGUILayout.PropertyField(mainStateProp, new GUIContent("Main State"));
            EditorGUILayout.PropertyField(parallelStateProp, new GUIContent("Parallel State"));
            Separator("");

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("Add Empty");
            if (GUILayout.Button("Add Sequence"))
            {
                vBranch.AddVisualizedSequence();
            }

            if (GUILayout.Button("Add Parallel"))
            {
                vBranch.AddVisualizedParallel();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void Separator(string label, Color lineColor = default(Color))
        {
            EditorGUILayout.Space();

            var headerStyle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
            EditorGUILayout.LabelField(label, headerStyle);

            if (lineColor == default)
                GUI.backgroundColor = Color.gray /*+ (Color.white + Color.white)*/;
            else
                GUI.backgroundColor = lineColor /*+ (Color.white + Color.white)*/;

            GUILayout.Box(GUIContent.none, GUILayout.Height(3), GUILayout.ExpandWidth(true));
            GUI.backgroundColor = Color.white;
        }
    }
}