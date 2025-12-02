using BehaviourGraph.States;

namespace BehaviourGraph.Conditions
{
    public class ISStateEnded : Condition
    {
        public ISStateEnded(IEndableState state)
        {
            _state = state;
        }
        
        private IEndableState _state;

        public override UpdateStatus ConditionUpdate()
        {
            return _state.EndCondition() == UpdateStatus.Successed ? UpdateStatus.Successed : UpdateStatus.Failure;
        }
    }
}