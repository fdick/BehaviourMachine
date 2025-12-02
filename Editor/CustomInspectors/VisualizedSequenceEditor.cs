using BehaviourGraph.Visualizer;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph.Editor
{
    [CustomEditor(typeof(VisualizedSequence))]
    public class VisualizedSequenceEditor : UnityEditor.Editor
    {
        private SerializedProperty resetBoolProp;
        private SerializedProperty startEvent;
        private SerializedProperty endEvent;
        private SerializedProperty statesProp;
        private SerializedProperty linksProp;

        private void OnEnable()
        {
            startEvent = serializedObject.FindProperty($"<OnStartState>k__BackingField");
            endEvent = serializedObject.FindProperty("<OnEndState>k__BackingField");
            resetBoolProp = serializedObject.FindProperty("_resetStateAtStart");
            statesProp = serializedObject.FindProperty("_states");
            linksProp = serializedObject.FindProperty("_links");
        }

        public override void OnInspectorGUI()
        {
            var vBranch = target as VisualizedSequence;
            if (vBranch == null)
                return;

            vBranch.FriendlyName = EditorGUILayout.TextField(new GUIContent("Friendly Name"), vBranch.FriendlyName);

            EditorGUILayout.Space();
            vBranch.Tag = EditorGUILayout.TextField(new GUIContent("Tag"), vBranch.Tag);
            EditorGUILayout.PropertyField(resetBoolProp, new GUIContent("Reset State At Start"));
            
            EditorGUILayout.PropertyField(startEvent, new GUIContent("On Start"));
            EditorGUILayout.PropertyField(endEvent, new GUIContent("On End"));

            Separator("Content");
            EditorGUILayout.PropertyField(statesProp, new GUIContent("States"));
            EditorGUILayout.PropertyField(linksProp, new GUIContent("Links"));
            vBranch.startableStateID =
                EditorGUILayout.IntField(new GUIContent("Start State ID"), vBranch.startableStateID);
            vBranch.startableStateID = Mathf.Clamp(vBranch.startableStateID, 0,
                statesProp.arraySize == 0 ? 0 : statesProp.arraySize - 1);
            Separator("");

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("Add Empty");
            if (GUILayout.Button("Add State") )
            {
               vBranch.AddVisualizedState();
            }


            if (GUILayout.Button("Add Link"))
            {
                vBranch.AddVisualizedLink();

            }

            if (GUILayout.Button("Add Sequence"))
            {
                vBranch.AddVisualizedSequence();
            }
            
            if (GUILayout.Button("Add Parallel"))
            {
                vBranch.AddVisualizedParallel();
            }
            
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            
            var s = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight};
            EditorGUILayout.LabelField("Find Exists", s);

            if (GUILayout.Button("Get States") )
            {
                vBranch.GetVisualizedStates();
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