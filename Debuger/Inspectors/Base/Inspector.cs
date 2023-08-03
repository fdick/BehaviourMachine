namespace BehaviourGraph.Visualizer
{
    public abstract class InspectorBase { }

    public abstract class Inspector<T> : InspectorBase where T: class
    {
        public abstract string Visualize(T node);
    }
}