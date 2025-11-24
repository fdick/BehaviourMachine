namespace BehaviourGraph.Trees
{
    public interface IBranch
    {
        public void StartBranch();
        public void OnUpdateBranch();
        public void OnFixedUpdateBranch();
        public void OnLateUpdateBranch();
        protected void OnStartBranch();
        protected void OnEndBranch();
    }
}