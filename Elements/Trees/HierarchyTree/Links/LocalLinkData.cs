using System.Collections.Generic;
using BehaviourGraph.States;

namespace BehaviourGraph.Trees
{
    public class LocalLinkData
    {
        public Dictionary<IState, List<Transition>> toStatesTransitions;
    }
}