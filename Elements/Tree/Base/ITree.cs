namespace BehaviourGraph.Trees
{
    public interface ITree
    {
        public void AwakeTree();
        public void StartTree();
        public UpdateStatus UpdateTree();
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
        public AIBehaviourGraph GetGraph();

        public string FriendlyName { get; }
    }
}
