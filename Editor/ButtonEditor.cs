using UnityEditor;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace BehaviourGraph.Visualizer
{
    [CustomEditor(typeof(Object), true, isFallback = false)]
    [CanEditMultipleObjects]
    public class ButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (var target in targets)
            {
                var mis = target.GetType().GetMethods().Where(m => m.GetCustomAttributes(true).Any(a => a.GetType() == typeof(InspectorButtonAttribute)));

                if (mis != null)
                {
                    foreach (var mi in mis)
                    {
                        if (mi == null)
                            return;

                        var attribute = (InspectorButtonAttribute)mi.GetCustomAttribute(typeof(InspectorButtonAttribute));

                        if (GUILayout.Button(attribute.text))
                        {
                            mi.Invoke(target, null);
                        }
                    }
                }
            }
        }

    }
}
