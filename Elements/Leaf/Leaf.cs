using System;
using System.Collections;
using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph
{
    public class Leaf : ILeaf, IDisposable
    {
        public string FriendlyName { get; set; } = "Leaf";
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
            _lastProcCD = Time.time;
        }


        public virtual LeafStatus OnUpdate()
        {
            return LeafStatus.Failure;
        }

        public virtual void OnEnd()
        {
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
            return Time.time >= _lastProcCD + duration || _lastProcCD == 0;
        }
    }
}