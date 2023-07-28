using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    public abstract class VisualizedLeaf : VisualizedBranch
    {
        public abstract Leaf GetInstance();
    }
}
