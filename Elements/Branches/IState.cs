using System;
using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph.States
{
    public interface IState : IPartOfMachine, IDisposable
    {
        public string Tag { get; set; }
        
        
        public Action<Transition> OnEnter { get; set; }
        public Action OnExit { get; set; }
        public void Enter(Transition transition = null);
        public void Exit();
        
        public void SetGameobject(GameObject go);
        public bool CheckCD(float duration);
        public float GetRemainingCD();
        
        public void InitializeState();
    }
}