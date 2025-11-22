using BehaviourGraph.Visualizer;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph.Editor
{
    [CustomEditor(typeof(VisualizedEmptyHierarchyTree))]
    public class VisualizedHierarchyTreeEditor : UnityEditor.Editor
    {
        private SerializedProperty resetBoolProp;
        private SerializedProperty leafsProp;
        private SerializedProperty linksProp;

        private void OnEnable()
        {
            resetBoolProp = serializedObject.FindProperty("resetStateAtStart");
            leafsProp = serializedObject.FindProperty("leafs");
            linksProp = serializedObject.FindProperty("links");
        }

        public override void OnInspectorGUI()
        {
            var vTree = target as VisualizedEmptyHierarchyTree;
            if (vTree == null)
                return;

            vTree.FriendlyName = EditorGUILayout.TextField(new GUIContent("Friendly Name"), vTree.FriendlyName);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(resetBoolProp, new GUIContent("Reset State At Start"));

            Separator("Content");
            EditorGUILayout.PropertyField(leafsProp, new GUIContent("Leafs"));
            EditorGUILayout.PropertyField(linksProp, new GUIContent("Links"));
            vTree.startableLeaf_ID =
                EditorGUILayout.IntField(new GUIContent("Start Leaf ID"), vTree.startableLeaf_ID);
            vTree.startableLeaf_ID = Mathf.Clamp(vTree.startableLeaf_ID, 0,
                leafsProp.arraySize == 0 ? 0 : leafsProp.arraySize - 1);
            Separator("");

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("Add Empty");
            if (GUILayout.Button("Add Leaf") )
            {
               vTree.AddVisualizedLeaf();
            }


            if (GUILayout.Button("Add Link"))
            {
                vTree.AddVisualizedLink();

            }

            if (GUILayout.Button("Add Branch"))
            {
                vTree.AddVisualizedBranch();
            }
            
            if (GUILayout.Button("Add Parallel Branch"))
            {
                vTree.AddVisualizedBranch();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            
            var s = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight};
            EditorGUILayout.LabelField("Find Exists", s);

            if (GUILayout.Button("Get Leafs") )
            {
                vTree.GetVisualizedLeafs();
            }


            if (GUILayout.Button("Get Links"))
            {
                vTree.GetVisualizedLinks();

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