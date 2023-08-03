using System;
using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph
{
    public class Leaf : ILeaf, IDisposable
    {
        public string FriendlyName { get; set; } = "Leaf";
        public bool IsActive { get; protected set; }
        protected GameObject gameObject;
        private float _lastProcCD;

        public Leaf()
        {
            FriendlyName = this.ToString();
        }

        public Action<ConditionData> OnStarting { get; set; }
        public Action OnEnded { get; set; }



        public virtual void OnAwake()
        {

        }

        public virtual void OnStart(ConditionData condData)
        {
            OnStarting?.Invoke(condData);
            IsActive = true;
        }


        public virtual UpdateStatus OnUpdate()
        {
            return UpdateStatus.Failure;
        }

        public virtual void OnEnd()
        {
            _lastProcCD = Time.time;
            OnEnded?.Invoke();
        }

        public void SetGameobject(GameObject go)
        {
            gameObject = go;
        }

        public void Dispose()
        {
            OnStarting = null;
            OnEnded = null;
        }

        public bool CheckCD(float duration)
        {
            return !IsActive && (Time.time >= _lastProcCD + duration || _lastProcCD == 0);
        }
    }
}