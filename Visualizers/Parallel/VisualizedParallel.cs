using System;
using BehaviourGraph.States;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public class VisualizedParallel : VisualizedState
    {
        public VisualizedState mainState;
        public VisualizedState parallelState;
        
    //     
    // public ITree GetInstance(BehaviourMachine graph)
    //     {
    //         //main leaf
    //         var mainLf = mainLeaf is IVisualizedTree mt ? (ILeaf)mt.GetInstance(graph) : mainLeaf.GetInstance();
    //
    //         //set custom name for main leaf
    //         if (mainLeaf.FriendlyName != string.Empty)
    //             mainLf.FriendlyName = mainLeaf.FriendlyName;
    //
    //         //parallel leafs
    //         var lfs = new ILeaf[_parallelLeafs.Count];
    //         for (int i = 0; i < _parallelLeafs.Count; i++)
    //         {
    //             //if child leaf is tree
    //             if (_parallelLeafs[i] is IVisualizedTree lt)
    //             {
    //                 var childTree = lt.GetInstance(graph);
    //
    //                 lfs[i] = (ILeaf)childTree;
    //             }
    //             else
    //             {
    //                 lfs[i] = _parallelLeafs[i].GetInstance();
    //             }
    //
    //             //leaf events
    //             if (_parallelLeafs[i].OnStartLeaf != null)
    //             {
    //                 int savedI = i;
    //                 lfs[i].OnEnter += (t) => _parallelLeafs[savedI].OnStartLeaf.Invoke();
    //             }
    //             if (_parallelLeafs[i].OnEndLeaf != null)
    //             {
    //                 int savedI = i;
    //                 lfs[i].OnExit += () => _parallelLeafs[savedI].OnEndLeaf.Invoke();
    //             }
    //
    //             //set custom names for parallel leafs
    //             if (_parallelLeafs[i].FriendlyName != string.Empty)
    //                 lfs[i].FriendlyName = _parallelLeafs[i].FriendlyName;
    //         }
    //
    //         var instance = new ParallelBranch(graph, mainLf, lfs);
    //
    //         if (OnStartLeaf != null)
    //             instance.OnEnter += (t) => OnStartLeaf?.Invoke();
    //
    //         if (OnEndLeaf != null)
    //             instance.OnExit += () => OnEndLeaf?.Invoke();
    //
    //         //set custom name for myself
    //         if (FriendlyName != string.Empty)
    //             instance.FriendlyName = FriendlyName;
    //
    //         return instance;
    //     }
        public override IState GetInstance(BehaviourMachine graph)
        {
            var main = mainState.GetInstance(graph);
            var par = parallelState.GetInstance(graph);

            var s1 = new Sequence(graph, new[] { main });
            var s2 = new Sequence(graph, new[] { par });
            
            var parallel = new Parallel(s1, s2);
            return parallel;
        }

        [InspectorButton("Add Sequence")]
        public void AddVisualizedSequence()
        {
            var go = new GameObject("Sequence1");
            VisualizedSequence s =
                (VisualizedSequence)go.AddComponent(typeof(VisualizedSequence));
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            if (mainState == null)
                mainState = s;
            else
                parallelState = s;
        }

        [InspectorButton("Add Parallel")]
        public void AddVisualizedParallel()
        {
            var go = new GameObject("Parallel");
            VisualizedParallel p =
                (VisualizedParallel)go.AddComponent(typeof(VisualizedParallel));
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            if (mainState == null)
                mainState = p;
            else
                parallelState = p;
        }

        // [InspectorButton("Get Parallel States")]
        // public void GetVisualizedParallelStates()
        // {
        //     List<VisualizedState> ls = new List<VisualizedState>();
        //     foreach (Transform c in transform)
        //     {
        //         if (c.name == _parallelStateName)
        //         {
        //             for (int i = 0; i < c.childCount; i++)
        //             {
        //                 var c2 = c.GetChild(i);
        //                 if (c2.TryGetComponent<VisualizedState>(out var outLeaf))
        //                     ls.Add(outLeaf);
        //             }
        //         }
        //         else
        //         {
        //             var cL = c.Find(_parallelStateName);
        //             if (cL == null)
        //                 continue;
        //
        //             for (int i = 0; i < cL.childCount; i++)
        //             {
        //                 var c2 = cL.GetChild(i);
        //                 if (c2.TryGetComponent<VisualizedState>(out var outLeaf))
        //                     ls.Add(outLeaf);
        //             }
        //         }
        //     }
        //
        //     parallelState.AddRange(ls);
        // }
    }
}