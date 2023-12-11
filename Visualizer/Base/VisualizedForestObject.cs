using BehaviourGraph.Trees;
using System;
using BehaviourGraph.Conditions;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public abstract class VisualizedForestObject : MonoBehaviour
    {
        [field: SerializeField] public string FriendlyName { get; set; }
    }

    [Serializable]
    public abstract class VisualizedTree : VisualizedForestObject, IVisualizedTree
    {
        public abstract ITree GetInstance(BehaviourMachine graph);
    }

    [Serializable]
    public abstract class VisualizedLeaf : VisualizedForestObject, IVisualizedLeaf
    {
        [field: SerializeField] public string Tag { get; set; }
        public abstract ILeaf GetInstance();
    }


    public interface IVisualizedForestObject
    {
        public string FriendlyName { get; set; }
    }

    public interface IVisualizedTree : IVisualizedForestObject
    {
        public ITree GetInstance(BehaviourMachine graph);
    }

    public interface IVisualizedLeaf : IVisualizedForestObject
    {
        public string Tag { get; set; }
        public ILeaf GetInstance();
    }

    public interface IVisualizedCondition : IVisualizedForestObject
    {
        public ICondition GetInstance(ITree tree);
    }
}