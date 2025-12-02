using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourGraph.Conditions;
using BehaviourGraph.Debug;
using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph.States
{
    public class Sequence : IState, IUpdatableState, IFixedUpdatableState, ILateUpdatableState, IEndableState,
        IDebugable
    {
        protected GameObject _gameObject;
        protected Transition _executedTransition;
        protected float _lastProcCD;
        protected bool _resetRunningStateToStartable;

        protected List<IState> _states = new List<IState>();
        protected List<Transition> _transitions = new List<Transition>();

        protected Dictionary<IState, EndLinkData> _endLinks = new Dictionary<IState, EndLinkData>();
        protected Dictionary<IState, LocalLinkData> _localLinks = new Dictionary<IState, LocalLinkData>();
        protected Dictionary<IState, GlobalLinkData> _globalLinks = new Dictionary<IState, GlobalLinkData>();

        public IState StartableState { get; set; }
        public IState RunningState { get; protected set; }

        /// <summary>
        /// Cash for last used state. Need for reset state.
        /// </summary>
        public IState CurrentState { get; protected set; }

        public Dictionary<IState, EndLinkData> EndLinks => _endLinks;
        public Dictionary<IState, LocalLinkData> LocalLinks => _localLinks;
        public Dictionary<IState, GlobalLinkData> GlobalLinks => _globalLinks;
        public List<IState> States => _states;
        public List<Transition> Transitions => _transitions;


        public string Tag { get; set; }
        public string FriendlyName { get; set; }
        public Guid ID { get; }
        public Action<Transition> OnEnter { get; set; }
        public Action OnExit { get; set; }

        /// <summary>
        /// Calls when running state is changing. IState param is a new state.
        /// </summary>
        public Action<IState> OnChangeRunningState { get; set; }

        /// <summary>
        /// Calls when executes any links by this sequence. (local + global + end links).
        /// </summary>
        public Action<Transition> OnExecuteLink { get; set; }

        public Action Breakpoint { get; set; }

        public UpdateStatus Status { get; private set; } = UpdateStatus.Failure;

        protected BehaviourMachine _parentGraph;


        public Sequence(BehaviourMachine graph, IState[] states, bool resetRunningStateToStartable = true)
        {
            _parentGraph = graph;

            ID = Guid.NewGuid();
            FriendlyName = nameof(Sequence);

            for (int i = 0; i < states.Length; i++)
            {
                AddState(states[i]);
            }

            if (_states == null || _states.Count == 0)
                UnityEngine.Debug.LogError(
                    $"{FriendlyName} (Gameobject: {_parentGraph.gameObject.name}) does not have states!");
            StartableState = _states[0];
            CurrentState = StartableState;

            _resetRunningStateToStartable = resetRunningStateToStartable;
        }

        public Sequence(BehaviourMachine graph, bool resetRunningStateToStartable = true)
        {
            _resetRunningStateToStartable = resetRunningStateToStartable;
            _parentGraph = graph;
            FriendlyName = ToString().Split('.').Last();
        }


        public void InitializeState()
        {
            for (int i = 0; i < _states.Count; i++)
            {
                _states[i].InitializeState();
            }
        }

        public void Enter(Transition transition = null)
        {
            OnEnter?.Invoke(transition);
            OnEnterSequence(transition);
            StartSequence();

            if (transition != null && transition.CooldownDuration > 0)
            {
                if (transition.CooldownType == CoolDownTypes.OnEnterDestinationState)
                    transition.SetCooldownTime();
                else
                    _executedTransition = transition;
            }
        }

        public void Exit()
        {
            if (_executedTransition != null && _executedTransition.CooldownDuration > 0)
            {
                if (_executedTransition.CooldownType == CoolDownTypes.OnExitDestinationState)
                {
                    _executedTransition.SetCooldownTime();
                    _executedTransition = null;
                }
            }

            OnExitSequence();
            EndSequence();

            _lastProcCD = Time.time;
            OnExit?.Invoke();
        }

        public void UpdateState()
        {
            if (RunningState == null)
                return;

            if (RunningState is IUpdatableState updatable)
            {
                updatable.UpdateState();
            }

            OnUpdateState();
        }

        public void FixedUpdateState()
        {
            if (RunningState == null)
                return;

            if (RunningState is IFixedUpdatableState fixedUpdatable)
            {
                fixedUpdatable.FixedUpdateState();
            }

            OnFixedUpdateState();
        }

        public void LateUpdateState()
        {
            if (RunningState == null)
                return;

            //late update
            if (RunningState is ILateUpdatableState lateUpdatable)
            {
                lateUpdatable.LateUpdateState();
            }

            OnLateUpdateState();

            UpdateTransition();
        }

        private void UpdateTransition()
        {
            if (RunningState == null)
                return;

            UpdateStatus sequStatus = UpdateStatus.Failure;
            bool noGlobalLinks = false;
            bool noLocalLinks = false;

            //global links update
            if (_globalLinks.Count > 0)
            {
                foreach (var d in _globalLinks)
                {
                    foreach (var c in d.Value.toStateTransitions)
                    {
                        if (RunningState == d.Key)
                            continue;
                        if (c.ExecutedCondition.ConditionUpdate() == UpdateStatus.Successed && c.CheckCooldown())
                        {
                            c.FromState = RunningState;
                            OnExecuteLink?.Invoke(c);

                            ChangeRunningState(d.Key, c);
                            return;
                        }
                    }
                }
            }
            else
            {
                noGlobalLinks = true;
            }

            //local links update
            if (_localLinks.TryGetValue(RunningState, out var to))
            {
                foreach (var d in to.toStatesTransitions)
                {
                    foreach (var t in d.Value)
                    {
                        if (t.ExecutedCondition.ConditionUpdate() == UpdateStatus.Successed && t.CheckCooldown())
                        {
                            OnExecuteLink?.Invoke(t);
                            t.FromState = RunningState;
                            ChangeRunningState(d.Key, t);
                            return;
                        }
                    }
                }
            }
            else
            {
                noLocalLinks = true;
            }

            //end update
            if (_endLinks.TryGetValue(RunningState, out var toState)
                && RunningState is IEndableState endable)
            {
                sequStatus = endable.EndCondition();

                if (sequStatus == UpdateStatus.Successed && toState.toStateTransition.CheckCooldown())
                {
                    OnExecuteLink?.Invoke(toState.toStateTransition);
                    ChangeRunningState(toState.toStateTransition.ToState, toState.toStateTransition);
                    return;
                }
            }
            //we should always check end states without others links
            else if (RunningState is IEndableState endState && noGlobalLinks && noLocalLinks)
            {
                Status = endState.EndCondition();
            }
        }

        public UpdateStatus EndCondition()
        {
            return Status;
        }

        public void SetGameobject(GameObject go)
        {
            _gameObject = go;
        }

        public bool CheckCD(float duration)
        {
            return Time.time >= _lastProcCD + duration || _lastProcCD == 0;
        }

        public float GetRemainingCD()
        {
            return Time.time - _lastProcCD;
        }

        public IState[] GetStates() => States.ToArray();
        public BehaviourMachine GetGraph() => _parentGraph;

        public int GetStateIndex(IState state)
        {
            if (state == null)
                return -1;
            return _states.IndexOf(state);
        }

        public void Dispose()
        {
            foreach (var l in _states)
            {
                if (l is IDisposable tDis)
                    tDis.Dispose();
            }

            EndLinks.Clear();
            GlobalLinks.Clear();
            LocalLinks.Clear();

            OnChangeRunningState = null;
            OnExecuteLink = null;
            Breakpoint = null;
            OnEnter = null;
            OnExit = null;
        }

        protected virtual void OnEnterSequence(Transition transition)
        {
        }

        protected virtual void OnExitSequence()
        {
        }

        public void AddState(IState state)
        {
            if (state == null)
                return;
            if (_states.Contains(state))
                return;

            state.SetGameobject(_parentGraph.CustomGameobject);
            _states.Add(state);
        }

        public void RemoveState(IState state)
        {
            //remove link from this state
            if (_localLinks.ContainsKey(state))
                _localLinks.Remove(state);
            //remove state
            if (_states.Contains(state))
                _states.Remove(state);
        }

        /// <summary>
        /// Add local link. Return unique ID for this link
        /// </summary>
        public Guid Link(IState origin, IState destination, ICondition condition,
            ExecutingTypes executingType = ExecutingTypes.Infinity, int maxExecuteQuantity = 1,
            float cooldownDuration = 0, CoolDownTypes cooldownType = CoolDownTypes.OnExitDestinationState)
        {
            if (origin == null || destination == null || condition == null)
            {
                throw new NullReferenceException(
                    $"{_parentGraph} : You try to add a confusing local link. Origin state is {origin}. Destination state is {destination}. Condition is {condition}");
            }

            if (!_states.Contains(origin))
            {
                throw new Exception($"{_parentGraph} : Behaviour machine doesn't contain the state {origin}");
            }

            if (!_states.Contains(destination))
            {
                throw new Exception($"{_parentGraph} : Behaviour machine doesn't contain the state {destination}");
            }


            //add link
            if (_localLinks.TryGetValue(origin, out var data))
            {
                var transition = new Transition(
                    condition, origin, destination, TransitionTypes.Local,
                    executingType, maxExecuteQuantity,
                    cooldownDuration, cooldownType);

                if (data.toStatesTransitions.TryGetValue(destination, out var conds))
                {
                    //i cant contains same condition
                    if (conds.Where((x) => x.ExecutedCondition == condition).Count() > 0)
                    {
                        UnityEngine.Debug.LogError($"{condition.FriendlyName} already contains!");
                        return default;
                    }

                    conds.Add(transition);
                }
                else
                    data.toStatesTransitions.Add(destination, new List<Transition> { transition });

                _transitions.Add(transition);
            }
            else
            {
                var linkData = new Dictionary<IState, List<Transition>>();
                var transition = new Transition(condition, origin, destination, TransitionTypes.Local,
                    executingType, maxExecuteQuantity,
                    cooldownDuration, cooldownType);
                linkData.Add(destination, new List<Transition> { transition });
                var localLinkData = new LocalLinkData() { toStatesTransitions = linkData };

                _localLinks.Add(origin, localLinkData);
                _transitions.Add(transition);
            }

            return condition.ID;
        }

        /// <summary>
        /// Add end link. If this state already has end link, that replace it
        /// </summary>
        public void Link(IState origin, IState destination,
            ExecutingTypes executingType = ExecutingTypes.Infinity, int maxExecuteQuantity = 1,
            float cooldownDuration = 0, CoolDownTypes cooldownType = CoolDownTypes.OnExitDestinationState)
        {
            if (origin == null || destination == null)
            {
                throw new NullReferenceException(
                    $"{_parentGraph} : You try to add a confusing end link. Origin state is {origin}. Destination state is {destination}");
            }

            if (origin is not IEndableState)
            {
                throw new NullReferenceException(
                    $"{_parentGraph} : Leaf {origin} is not endable!");
            }

            if (!_states.Contains(origin))
            {
                throw new Exception($"{_parentGraph} : Behaviour machine doesn't contain the state {origin}");
            }

            if (!_states.Contains(destination))
            {
                throw new Exception($"{_parentGraph} : Behaviour machine doesn't contain the state {destination}");
            }

            if (_endLinks.TryGetValue(origin, out var _))
                _endLinks.Remove(origin);

            var transition = new Transition(
                null, origin, destination, TransitionTypes.End,
                executingType, maxExecuteQuantity,
                cooldownDuration, cooldownType);
            var endLinkData = new EndLinkData() { toStateTransition = transition };
            _endLinks.Add(origin, endLinkData);
        }

        /// <summary>
        /// Add global link. Return unique ID for this link
        /// </summary>
        public Guid Link(IState toState, ICondition condition,
            ExecutingTypes executingType = ExecutingTypes.Infinity, int maxExecuteQuantity = 1,
            float cooldownDuration = 0, CoolDownTypes cooldownType = CoolDownTypes.OnExitDestinationState)
        {
            if (toState == null || condition == null)
            {
                throw new NullReferenceException(
                    $"{_parentGraph} : You try to add a confusing global link. State is {toState}. Condition is {condition}");
            }

            if (!_states.Contains(toState))
            {
                throw new Exception($"{_parentGraph} : Behaviour machine doesn't contain the state {toState}");
            }

            var transition = new Transition(
                condition, null, toState, TransitionTypes.Global,
                executingType, maxExecuteQuantity,
                cooldownDuration, cooldownType);

            if (_globalLinks.TryGetValue(toState, out var l))
                l.toStateTransitions.Add(transition);
            else
                _globalLinks.Add(toState,
                    new GlobalLinkData() { toStateTransitions = new List<Transition>() { transition } });

            _transitions.Add(transition);
            return condition.ID;
        }

        /// <summary>
        /// Remove end link
        /// </summary>
        public void RemoveEndLink(IState origin)
        {
            if (!_endLinks.ContainsKey(origin))
                return;

            var transition = _endLinks[origin];
            _transitions.Remove(transition.toStateTransition);

            _endLinks.Remove(origin);
        }

        /// <summary>
        /// Remove local link
        /// </summary>
        public void RemoveLink(IState origin, IState destination)
        {
            if (!_states.Contains(origin) ||
                !_states.Contains(destination) ||
                !_localLinks.ContainsKey(origin))
                return;

            if (_localLinks.TryGetValue(origin, out var data))
                if (!data.toStatesTransitions.ContainsKey(destination))
                    return;

            foreach (var t in data.toStatesTransitions[destination])
            {
                _transitions.Remove(t);
            }

            data.toStatesTransitions.Remove(destination);

            //delete empty link
            if (data.toStatesTransitions.Count == 0)
                _localLinks.Remove(origin);
        }

        /// <summary>
        /// Remove global link
        /// </summary>
        public void RemoveLink(IState globalState)
        {
            if (!_states.Contains(globalState))
                return;
            if (!_globalLinks.ContainsKey(globalState))
                return;

            foreach (var t in _globalLinks[globalState].toStateTransitions)
            {
                _transitions.Remove(t);
            }

            _globalLinks.Remove(globalState);
        }

        public void RemoveLink(Transition transition)
        {
            if (transition == null)
                return;
            if (!_transitions.Contains(transition))
                return;

            switch (transition.TransitionType)
            {
                case TransitionTypes.End:
                    RemoveEndLink(transition.FromState);
                    break;
                case TransitionTypes.Local:
                    RemoveLink(transition.FromState, transition.ToState);
                    break;
                case TransitionTypes.Global:
                    RemoveLink(transition.ToState);
                    break;
            }
        }

        public void RemoveLink(int transitionIndex)
        {
            if (transitionIndex < 0 || transitionIndex >= _transitions.Count)
            {
                throw new IndexOutOfRangeException(
                    $"{transitionIndex} out of range! Range is : 0 - {_transitions.Count}");
            }

            var t = _transitions[transitionIndex];
            RemoveLink(t);
        }

        /// <summary>
        /// Force enter to State. Find first state by T type. Only for this sequence childs. 
        /// </summary>
        public void ForceEnter<T>() where T : IState
        {
            var state = _states.FirstOrDefault(x => x.GetType() == typeof(T));

            if (state == null)
                return;

            var id = _states.IndexOf(state);
            ForceEnter(id);
        }

        /// <summary>
        /// Force enter to State. Only for this sequence childs. 
        /// </summary>
        public void ForceEnter(int stateID)
        {
            if (stateID >= _states.Count)
                return;

            var temporaryTransition = new Transition(null, RunningState, _states[stateID],
                TransitionTypes.End, ExecutingTypes.Infinity);
            ChangeRunningState(_states[stateID], temporaryTransition);
        }

        /// <summary>
        /// Find a first child state by type. Searching in all hierarchy.
        /// </summary>
        /// <typeparam name="T"> is finding type</typeparam>
        /// <returns></returns>
        public T QState<T>() where T : class, IState
        {
            foreach (var l in _states)
            {
                if (l is T tState)
                    return tState;
                if (l is Sequence t)
                {
                    var r = t.QState<T>();
                    if (r != null)
                        return r;
                }
            }

            return null;
        }

        /// <summary>
        /// Find a first child state by tag. Searching in all hierarchy.
        /// </summary>
        /// <param name="tag">Tag of a state.</param>
        public IState QState(string tag)
        {
            foreach (var l in _states)
            {
                if (string.Equals(l.Tag, tag))
                    return l;
                if (l is Sequence t)
                {
                    var r = t.QState(tag);
                    if (r != null)
                        return r;
                }
            }

            return null;
        }

        protected virtual void OnUpdateState()
        {
        }

        protected virtual void OnFixedUpdateState()
        {
        }

        protected virtual void OnLateUpdateState()
        {
        }

        private void AbortRunningState()
        {
            if (RunningState == null)
                return;

            RunningState.Exit();
            RunningState = null;
        }

        private void ApplyRunningState(IState state)
        {
            if (RunningState != null)
                return;
            CurrentState = state;
            RunningState = state;
        }

        private void StartSequence()
        {
            if (StartableState == null)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log($"{FriendlyName}: Startable state is null!");
#endif
                return;
            }

            if (!_resetRunningStateToStartable && CurrentState != null)
                ChangeRunningState(CurrentState, null);
            else
                ChangeRunningState(StartableState, null);
        }

        private void EndSequence()
        {
            //if running state is running than stop it
            AbortRunningState();
        }

        private void ChangeRunningState(IState newState, Transition transition = null)
        {
#if UNITY_EDITOR
            Breakpoint?.Invoke();
#endif
            OnChangeRunningState?.Invoke(newState);

            AbortRunningState();
            ApplyRunningState(newState);

            RunningState.Enter(transition);

            //remove transition if quantity executes equals max executes
            if (transition != null && transition.ExecutingType == ExecutingTypes.Custom)
            {
                transition.ExecutedTimes++;
                if (transition.ExecutedTimes >= transition.MaxExecuteQuantities)
                {
                    //remove end link
                    if (transition.ExecutedCondition == null)
                        RemoveEndLink(transition.FromState);
                    //remove global link
                    else if (transition.FromState == null)
                        RemoveLink(transition.ToState);
                    //remove local link
                    else
                        RemoveLink(transition.FromState, transition.ToState);
                }
            }
        }
    }
}