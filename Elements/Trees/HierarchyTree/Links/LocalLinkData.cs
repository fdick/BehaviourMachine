using System.Collections.Generic;

namespace BehaviourGraph.Trees
{
    public class LocalLinkData
    {
        public Dictionary<ILeaf, List<Transition>> toLeafsTransitions;
    }
}