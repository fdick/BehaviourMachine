using System;

namespace BehaviourGraph.Visualizer
{

    [AttributeUsage(AttributeTargets.Method)]
    public class InspectorButtonAttribute : Attribute
    {

        public string text;

        public InspectorButtonAttribute(string btnText)
        {
            this.text = btnText;
        }
    }
}
