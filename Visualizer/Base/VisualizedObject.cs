using BehaviourGraph.Trees;
using System;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public abstract class VisualizedObject : MonoBehaviour
    {
        [field: SerializeField] public string FriendlyName { get; set; }

    }

    [Serializable]
    public abstract class VisualizedTree : VisualizedObject, IVisualizedTree
    {
        public abstract ITree GetInstance(AIBehaviourGraph graph);
    }

    [Serializable]
    public abstract class VisualizedLeaf : VisualizedObject, IVisualizedLeaf
    {
        public abstract ILeaf GetInstance();
    }

    public interface IVisualizedTree
    {
        public ITree GetInstance(AIBehaviourGraph graph);
    }

    public interface IVisualizedLeaf
    {
        public ILeaf GetInstance();
    }

    public interface IVisualizedCondition
    {
        public IConditional GetInstance(ITree tree);
    }
}