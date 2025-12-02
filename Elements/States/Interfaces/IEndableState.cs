namespace BehaviourGraph.States
{
    public interface IEndableState
    {
        public UpdateStatus EndCondition();
    }
}