using System;
using BehaviourGraph.States;

namespace BehaviourGraph.Trees
{
    public class OakTree : ITree
    {
        public string FriendlyName { get; set; }
        public Guid ID { get; }
        public bool IsPaused { get; private set; }
        public UpdateStatus Status { get; }
        public Sequence Sequence { get; protected set; }

        
        protected BehaviourMachine _parentGraph;
        

        public OakTree(BehaviourMachine graph, Sequence sequence = null, bool resetRunningStateToStartable = true)
        {
            _parentGraph = graph;
            ID = Guid.NewGuid();
            FriendlyName = nameof(OakTree);
            
            if(sequence == null)
                sequence = new Sequence(graph, resetRunningStateToStartable);

            Sequence = sequence;
            Sequence.FriendlyName = "Root";
        }

        public void InitTree()
        {
            Sequence.InitializeState();
        }

        public void StartTree()
        {
            if(Sequence == null)
                return;
            
            Sequence.Enter();
        }
        
        public void StopTree()
        {
            if(Sequence == null)
                return;
            
            Sequence.Exit();
        }

        
        public void UpdateTree()
        {
            if(Sequence == null)
                return;
            
            if(IsPaused)
                return;
            
            Sequence.UpdateState();
        }

        public void FixedUpdateTree()
        {
            if(Sequence == null)
                return;
            
            if(IsPaused)
                return;
            
            Sequence.FixedUpdateState();
        }

        public void LateUpdateTree()
        {
            if(Sequence == null)
                return;
            
            if(IsPaused)
                return;
            
            Sequence.LateUpdateState();
        }
        
        public UpdateStatus EndCondition()
        {
            if(Sequence == null)
                return UpdateStatus.Failure;
            
            if(IsPaused)
                return UpdateStatus.Failure;

            return Sequence.EndCondition();
        }
        

        public void PauseTree()
        {
            if(Sequence == null)
                return;
            
            IsPaused = true;
        }

        public void UnPauseTree()
        {
            if(Sequence == null)
                return;
            
            IsPaused = false;
        }
        
        
        
        
        
        public void AddState(IState state)
        {
            Sequence?.AddState(state);
        }

        public void RemoveState(IState state)
        {
            Sequence?.RemoveState(state);
            
        }

        public IState GetRunningState()
        {
            return Sequence?.RunningState;
        }

        public IState GetStartableState()
        {
            return Sequence;
        }

        public IState[] GetStates()
        {
            return Sequence?.GetStates();
        }

        public BehaviourMachine GetGraph()
        {
            return _parentGraph;
        }

        public T QState<T>() where T : class, IState
        {
            return Sequence?.QState<T>();
        }

        public IState QState(string tag)
        {
            return Sequence?.QState(tag);
            
        }

        public void Dispose()
        {
            Sequence.Dispose();
        }
    }
}