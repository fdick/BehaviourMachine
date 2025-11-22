using BehaviourGraph.Visualizer;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph.Editor
{
    [CustomEditor(typeof(VisualizedEmptyHierarchyBranch))]
    public class VisualizedHierarchyBranchEditor : UnityEditor.Editor
    {
        private SerializedProperty resetBoolProp;
        private SerializedProperty startEvent;
        private SerializedProperty endEvent;
        private SerializedProperty leafsProp;
        private SerializedProperty linksProp;

        private void OnEnable()
        {
            startEvent = serializedObject.FindProperty($"<OnStartLeaf>k__BackingField");
            endEvent = serializedObject.FindProperty("<OnEndLeaf>k__BackingField");
            resetBoolProp = serializedObject.FindProperty("_resetStateAtStart");
            leafsProp = serializedObject.FindProperty("_leafs");
            linksProp = serializedObject.FindProperty("_links");
        }

        public override void OnInspectorGUI()
        {
            var vBranch = target as VisualizedEmptyHierarchyBranch;
            if (vBranch == null)
                return;

            vBranch.FriendlyName = EditorGUILayout.TextField(new GUIContent("Friendly Name"), vBranch.FriendlyName);

            EditorGUILayout.Space();
            vBranch.Tag = EditorGUILayout.TextField(new GUIContent("Tag"), vBranch.Tag);
            EditorGUILayout.PropertyField(resetBoolProp, new GUIContent("Reset State At Start"));
            
            EditorGUILayout.PropertyField(startEvent, new GUIContent("On Start"));
            EditorGUILayout.PropertyField(endEvent, new GUIContent("On End"));

            Separator("Content");
            EditorGUILayout.PropertyField(leafsProp, new GUIContent("Leafs"));
            EditorGUILayout.PropertyField(linksProp, new GUIContent("Links"));
            vBranch.startableLeafID =
                EditorGUILayout.IntField(new GUIContent("Start Leaf ID"), vBranch.startableLeafID);
            vBranch.startableLeafID = Mathf.Clamp(vBranch.startableLeafID, 0,
                leafsProp.arraySize == 0 ? 0 : leafsProp.arraySize - 1);
            Separator("");

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("Add Empty");
            if (GUILayout.Button("Add Leaf") )
            {
               vBranch.AddVisualizedLeaf();
            }


            if (GUILayout.Button("Add Link"))
            {
                vBranch.AddVisualizedLink();

            }

            if (GUILayout.Button("Add Branch"))
            {
                vBranch.AddVisualizedBranch();
            }
            
            if (GUILayout.Button("Add Parallel Branch"))
            {
                vBranch.AddVisualizedBranch();
            }
            
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            
            var s = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight};
            EditorGUILayout.LabelField("Find Exists", s);

            if (GUILayout.Button("Get Leafs") )
            {
                vBranch.GetVisualizedLeafs();
            }


            if (GUILayout.Button("Get Links"))
            {
                vBranch.GetVisualizedLinks();

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