using System;
using BehaviourGraph.Conditions;
using BehaviourGraph.States;
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
        OnEnterDestinationState,
        OnExitDestinationState,
    }

    public class Transition
    {
        public Transition(ICondition executedCondition, IState fromState, IState toState, TransitionTypes transitionType,
            ExecutingTypes executingType, int maxExecuteQuantities = 1, 
            float cooldownDuration = 0, CoolDownTypes cooldownType = CoolDownTypes.OnExitDestinationState)
        {
            this.ExecutedCondition = executedCondition;
            this.FromState = fromState;
            this.ToState = toState;
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
        public IState FromState { get; set; }
        public IState ToState { get; set; }

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