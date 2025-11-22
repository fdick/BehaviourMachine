using BehaviourGraph.Trees;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourGraph.Visualizer
{
    public enum LinkType
    {
        FromTo,
        Ended,
        Global,
    }

    public class VisualizedLink : VisualizedForestObject
    {
        public LinkType linkType;
        
        public ExecutingTypes executingType;
        public int executesQuantity = 1;

        public CoolDownTypes setCoolDownOn = CoolDownTypes.OnExitDestinationLeaf;
        public float coolDown = 0;
        
        [FormerlySerializedAs("froms2")] public VisualizedLeaf[] froms;
        [FormerlySerializedAs("to2")] public VisualizedLeaf to;
        public VisualizedCondition condition;
        
        public void AddVisualiseCondition()
        {
            var conditionName = "Condition";
            var go = new GameObject(conditionName);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
        }

        public void AddSelector()
        {
            var go = new GameObject("Selector");
            VisualizedSelector select = (VisualizedSelector)go.AddComponent(typeof(VisualizedSelector));
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            condition = select;
        }

        public void AddSequencer()
        {
            var go = new GameObject("Sequencer");
            VisualizedSequencer sequen = (VisualizedSequencer)go.AddComponent(typeof(VisualizedSequencer));

            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            condition = sequen;
        }

        private void OnValidate()
        {
            FriendlyName = "Not using";
        }
    }
}