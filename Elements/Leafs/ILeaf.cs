using System;
using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph
{
    public enum UpdateStatus
    {
        Successed,
        Failure,
        Running
    }

    public interface ILeaf : IForestObject
    {
        public string Tag { get; set; }
        public Action<Transition> OnEnter { get; set; }
        public Action OnExit { get; set; }
        public void InitLeaf();
        public void EnterLeaf(Transition transition = null);
        public void ExitLeaf();
        public void SetGameobject(GameObject go);
        public bool CheckCD(float duration);
    }
}