namespace BehaviourGraph.Trees
{
    public interface ITree : IForestObject
    {
        public void AwakeTree();
        public void StartTree();
        
        public void UpdateTree();
        public void FixedUpdateTree();
        public UpdateStatus LateUpdateTree();
        
        public void EndTree();
        public void PauseTree();
        public void UnPauseTree();
        public bool IsPaused { get; }
        public UpdateStatus Status { get; }

        public void AddLeaf(ILeaf leaf);
        public void RemoveLeaf(ILeaf leaf);

        public ILeaf GetRunningLeaf();
        public ILeaf GetStartableLeaf();
        public ILeaf[] GetLeafs();
        public BehaviourMachine GetGraph();
    }
}
