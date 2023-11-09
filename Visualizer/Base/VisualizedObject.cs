using BehaviourGraph.Trees;
using System;
using BehaviourGraph.Conditions;
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
        public abstract ITree GetInstance(BehaviourMachine graph);
    }

    [Serializable]
    public abstract class VisualizedLeaf : VisualizedObject, IVisualizedLeaf
    {
        public abstract ILeaf GetInstance();
    }

    public interface IVisualizedTree
    {
        public ITree GetInstance(BehaviourMachine graph);
    }

    public interface IVisualizedLeaf
    {
        public ILeaf GetInstance();
    }

    public interface IVisualizedCondition
    {
        public ICondition GetInstance(ITree tree);
    }
}