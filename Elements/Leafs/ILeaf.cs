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
        public Action<Transition> OnEnter { get; set; }
        public Action OnExit { get; set; }
        public void InitLeaf();
        public void EnterLeaf(Transition activatedLink = null);
        public void ExitLeaf();
        public void SetGameobject(GameObject go);
        public bool CheckCD(float duration);
    }
}