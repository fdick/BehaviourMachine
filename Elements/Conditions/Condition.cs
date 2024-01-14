using System;
using UnityEditor;

namespace BehaviourGraph.Conditions
{
    public class Condition : ICondition, IDisposable
    {
        public Condition()
        {
            FriendlyName = this.ToString();
            ID = new Guid();
        }

        public Condition(Func<bool> condition)
        {
            FriendlyName = this.ToString();
            ID = new Guid();
            _condition = condition;
        }

        public string FriendlyName { get; set; }
        public Guid ID { get; }
        private Func<bool> _condition;

        public virtual UpdateStatus ConditionUpdate()
        {
            return _condition?.Invoke() == true ? UpdateStatus.Successed : UpdateStatus.Failure;
        }

        public void Dispose()
        {
            _condition = null;
        }
    }
}