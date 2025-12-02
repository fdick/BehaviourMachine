using BehaviourGraph.Trees;
using BehaviourGraph.Visualizer;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph.Editor
{
    [CustomEditor(typeof(VisualizedLink))]
    public class VisualizedLinkEditor : UnityEditor.Editor
    {
        private SerializedProperty linkTypeProp;
        private SerializedProperty executingTypeProp;
        private SerializedProperty executesQuantityProp;
        private SerializedProperty setCoolDownTypeProp;
        private SerializedProperty fromsProp;
        private SerializedProperty toProp;
        private SerializedProperty conditionProp;

        private void OnEnable()
        {
            linkTypeProp = serializedObject.FindProperty("linkType");
            executingTypeProp = serializedObject.FindProperty("executingType");
            executesQuantityProp = serializedObject.FindProperty("executesQuantity");
            setCoolDownTypeProp = serializedObject.FindProperty("setCoolDownOn");
            fromsProp = serializedObject.FindProperty("froms");
            toProp = serializedObject.FindProperty("to");
            conditionProp = serializedObject.FindProperty("condition");
        }

        public override void OnInspectorGUI()
        {
            var vLink = target as VisualizedLink;
            if (vLink == null)
                return;

            // vLink.FriendlyName = EditorGUILayout.TextField(new GUIContent("Friendly Name"), vLink.FriendlyName);

            Separator("Transition");
            EditorGUILayout.PropertyField(linkTypeProp, new GUIContent("Link Type"));
            EditorGUILayout.Space();
            if (vLink.linkType != LinkType.Global)
                EditorGUILayout.PropertyField(fromsProp, new GUIContent("Froms"));

            EditorGUILayout.PropertyField(toProp, new GUIContent("To"));
            if (vLink.linkType != LinkType.HasEnded)
                EditorGUILayout.PropertyField(conditionProp, new GUIContent("Condition"));

            Separator("Transition Executes");
            EditorGUILayout.PropertyField(executingTypeProp, new GUIContent("Executing Type"));
            if (vLink.executingType != ExecutingTypes.Infinity)
            {
                EditorGUILayout.PropertyField(executesQuantityProp, new GUIContent("Executes Quantity"));
                if (vLink.executesQuantity < 0)
                    vLink.executesQuantity = 0;
            }

            Separator("Transition Cooldown");
            EditorGUILayout.PropertyField(setCoolDownTypeProp, new GUIContent("Set Cooldown On"));
            vLink.coolDown = EditorGUILayout.FloatField("Cooldown Duration", vLink.coolDown);
            if (vLink.coolDown < 0)
                vLink.coolDown = 0;

            Separator("");

            if (GUILayout.Button("Add Condition"))
            {
                vLink.AddVisualiseCondition();
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Selector"))
            {
                vLink.AddSelector();
            }

            if (GUILayout.Button("Add Sequencer"))
            {
                vLink.AddSequencer();
            }

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