﻿using System.Collections;
using System.Linq;

namespace BehaviourGraph.Conditions
{
    /// <summary>
    /// Like <or> for all child conditions
    /// </summary>
    public class Selector : CombinerConditions
    {
        public Selector(params IConditional[] conditions)
        {
            Conditions = conditions.ToList();

            FriendlyName = "( ";
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (i > 0)
                    FriendlyName += " || ";
                FriendlyName += Conditions[i].FriendlyName;
                if (i == Conditions.Count - 1)
                    FriendlyName += " )";
            }
        }

        public override UpdateStatus OnUpdate()
        {
            foreach (var c in Conditions)
            {
                if (c.OnUpdate() == UpdateStatus.Successed)
                    return UpdateStatus.Successed;
            }
            return UpdateStatus.Failure;
        }
    }
}