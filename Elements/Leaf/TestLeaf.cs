using System.Collections;
using UnityEngine;

namespace BehaviourGraph.Leafs
{
    public class TestLeaf : Leaf
    {
        public TestLeaf()
        {

        }

        public override LeafStatus OnUpdate()
        {
            UnityEngine.Debug.Log("cond");
            return base.OnUpdate();
        }
    }
}