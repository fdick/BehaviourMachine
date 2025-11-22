using BehaviourGraph.Visualizer;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph.Editor
{
    [CustomEditor(typeof(VisualizedEmptyParallelBranch))]
    public class VisualizedParallelBranchEditor : UnityEditor.Editor
    {
        private SerializedProperty leafsProp;
        private SerializedProperty mainLeafProp;

        private void OnEnable()
        {
            mainLeafProp = serializedObject.FindProperty("mainLeaf");
            leafsProp = serializedObject.FindProperty("_parallelLeafs");
        }

        public override void OnInspectorGUI()
        {
            var vBranch = target as VisualizedEmptyParallelBranch;
            if (vBranch == null)
                return;

            vBranch.FriendlyName = EditorGUILayout.TextField(new GUIContent("Friendly Name"), vBranch.FriendlyName);

            EditorGUILayout.Space();
            vBranch.Tag = EditorGUILayout.TextField(new GUIContent("Tag"), vBranch.Tag);

            Separator("Content");
            EditorGUILayout.PropertyField(mainLeafProp, new GUIContent("Main Leaf"));
            EditorGUILayout.PropertyField(leafsProp, new GUIContent("Parallel Leafs"));
            Separator("");

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("Add Empty");
            if (GUILayout.Button("Add Main Branch") )
            {
               vBranch.AddVisualizedMainBranch();
            }


            if (GUILayout.Button("Add Parallel Leaf"))
            {
                vBranch.AddVisualizedParallelLeaf();

            }
            
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            
            var s = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight};
            EditorGUILayout.LabelField("Find Exists", s);

            if (GUILayout.Button("Get Parallel Leafs") )
            {
                vBranch.GetVisualizedParallelLeafs();
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