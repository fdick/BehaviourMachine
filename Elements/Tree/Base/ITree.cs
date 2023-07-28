namespace BehaviourGraph.Trees
{
    public enum MachineStatus
    {
        Stoped,
        Updating,
    }


    public class MachineStateData
    {
        public float updateTick = 0.05f;
        public string machineName = "Machine State";
    }

    public interface ITree 
    { 
        public void StartTree();
        public LeafStatus UpdateTree();
        public void EndTree();
        public void PauseTree();
        public void UnPauseTree();

        public void AddLeaf(ILeaf leaf);
        public void RemoveLeaf(ILeaf leaf);

        //public void Link(ILeaf origin, ILeaf destination, IConditional condition);
        //public void RemoveLink(ILeaf origin, ILeaf destination);

        public ILeaf GetRunningLeaf();
        //public ILeaf GetEndedRunningLeaf();
        public ILeaf GetStartableLeaf();
        public ILeaf[] GetLeafs();
        public AIBehaviourGraph GetGraph();

        //public bool HasEndableLeaf();
        public string FriendlyName { get; }


    }
}
