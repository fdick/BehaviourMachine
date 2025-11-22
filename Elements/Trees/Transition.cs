using System;
using BehaviourGraph.Conditions;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph.Trees
{
    public enum TransitionTypes
    {
        End,
        Local,
        Global,
    }
    public enum ExecutingTypes
    {
        Infinity,
        Custom,
    }

    public enum CoolDownTypes
    {
        OnEnterDestinationLeaf,
        OnExitDestinationLeaf,
    }

    public class Transition
    {
        public Transition(ICondition executedCondition, ILeaf fromLeaf, ILeaf toLeaf, TransitionTypes transitionType,
            ExecutingTypes executingType, int maxExecuteQuantities = 1, 
            float cooldownDuration = 0, CoolDownTypes cooldownType = CoolDownTypes.OnExitDestinationLeaf)
        {
            this.ExecutedCondition = executedCondition;
            this.FromLeaf = fromLeaf;
            this.ToLeaf = toLeaf;
            this.TransitionType = transitionType;
            this.ExecutingType = executingType;
            this.MaxExecuteQuantities = maxExecuteQuantities;
            this.CooldownType = cooldownType;
            this.CooldownDuration = cooldownDuration;
            ExecutedTimes = 0;
        }

        public Guid ID => ExecutedCondition?.ID ?? default;

        public TransitionTypes TransitionType { get; }
        public ICondition ExecutedCondition { get; }
        public ILeaf FromLeaf { get; set; }
        public ILeaf ToLeaf { get; set; }

        public ExecutingTypes ExecutingType { get; }
        public int MaxExecuteQuantities { get; }
        public int ExecutedTimes { get; set; }

        public CoolDownTypes CooldownType { get; }
        public float CooldownDuration { get; }
        private float _setCooldownTime;

        public void SetCooldownTime() => _setCooldownTime = Time.time;

        public bool CheckCooldown() => _setCooldownTime == 0 || Time.time >= _setCooldownTime + CooldownDuration;
    }
}