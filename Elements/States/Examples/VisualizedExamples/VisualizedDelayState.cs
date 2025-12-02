using BehaviourGraph.States;

namespace BehaviourGraph.Visualizer
{
    public class VisualizedDelayState : VisualizedState
    {
        public System.Single delayTime;


        public override IState GetInstance(BehaviourMachine graph)
        {
            return new DelayState(delayTime);
        }
    }
}