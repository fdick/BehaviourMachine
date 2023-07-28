using BehaviourGraph.Trees;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    public static class VisualizatorGenerator 
    {
        private const string TAB = "    ";


        [MenuItem("CustomTools/AIBehaviourGraph/Generate Visualized Object")]
        public static void Generate()
        {
            if (Selection.activeObject == null)
                return;
            var selectedObj = Selection.activeObject;
            var selectedFileType = Type.GetType($"{selectedObj.name}", true);

            if (!typeof(ITree).IsAssignableFrom(selectedFileType) &&
                !typeof(ILeaf).IsAssignableFrom(selectedFileType) &&
                !typeof(IConditional).IsAssignableFrom(selectedFileType))
            {
                UnityEngine.Debug.Log("This script can not be origin for generator!");
                return;
            }

            var scriptName = "Visualized" + selectedObj.name;
            var directory = $"{Application.dataPath}";
            var scriptPath = EditorUtility.SaveFilePanel("Save as", directory, scriptName, "cs");

            var constructors = selectedFileType.GetConstructors();

            if (constructors == null || constructors.Length == 0)
                return;

            var constructorParameters = constructors[0].GetParameters();
            var script = string.Empty;

            if (typeof(ITree).IsAssignableFrom(selectedFileType))
                script = GetBranchScript(constructorParameters, selectedFileType);
            else if (typeof(IConditional).IsAssignableFrom(selectedFileType))
                script = GetCondiitonScript(constructorParameters, selectedFileType);
            else if (typeof(ILeaf).IsAssignableFrom(selectedFileType))
                script = GetLeafScript(constructorParameters, selectedFileType);


            File.WriteAllText(scriptPath, script);
        }


        private static string GetCondiitonScript(ParameterInfo[] constructorParameters, Type selectedFileType)
        {
            var script = string.Empty;
            var scriptName = string.Empty;
            var bodyFields = string.Empty;
            var getInstanceMethod = string.Empty;


            var paramsForMethodWithoutType = string.Empty;
            for (int i = 0; i < constructorParameters.Length; i++)
            {
                var p = constructorParameters[i];
                if (typeof(ILeaf).IsAssignableFrom(p.ParameterType))
                    paramsForMethodWithoutType += $"branch.Leafs[{p.Name}_ID]";
                else 
                    paramsForMethodWithoutType += $"{constructorParameters[i].Name}";

                if (i < constructorParameters.Length - 1)
                    paramsForMethodWithoutType += ", ";
            }


            foreach (var p in constructorParameters)
            {
                if (typeof(ILeaf).IsAssignableFrom(p.ParameterType))
                    bodyFields += $"{TAB}public int {p.Name}_ID;\n";
                else
                    bodyFields += $"{TAB}public {p.ParameterType} {p.Name};\n";
            }

            getInstanceMethod =
$@"
{TAB}public override IConditional GetInstance(HierarchyBranch branch)
{TAB}{{
{TAB}{TAB}return new {selectedFileType}({paramsForMethodWithoutType});
{TAB}}}";

            scriptName = "Visualized" + selectedFileType;

            script =
$@"
using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.Trees;

public class {scriptName} : VisualizedCondition
{{
{bodyFields}
{getInstanceMethod}
}}";

            return script;
        }

        private static string GetBranchScript(ParameterInfo[] constructorParameters, Type selectedFileType)
        {
            var script = string.Empty;
            var scriptName = string.Empty;
            var bodyFields = string.Empty;
            var getInstanceMethod = string.Empty;

            foreach (var p in constructorParameters)
            {
                if (p.ParameterType == typeof(AIBehaviourGraph))
                    continue;
                bodyFields += $"{TAB}public {p.ParameterType} {p.Name};\n";
            }

            var paramsForMethod = string.Empty;
            for (int i = 0; i < constructorParameters.Length; i++)
            {
                paramsForMethod += $"{constructorParameters[i].ParameterType} {constructorParameters[i].Name}";
                if (i < constructorParameters.Length - 1)
                    paramsForMethod += ", ";
            }

            var paramsForMethodWithoutType = string.Empty;
            for (int i = 0; i < constructorParameters.Length; i++)
            {
                paramsForMethodWithoutType += $"{constructorParameters[i].Name}";
                if (i < constructorParameters.Length - 1)
                    paramsForMethodWithoutType += ", ";
            }


            getInstanceMethod =
$@"
{TAB}public override HierarchyBranch GetInstance(AIBehaviourGraph graph)
{TAB}{{
{TAB}{TAB}return new {selectedFileType}({paramsForMethodWithoutType});
{TAB}}}";

            scriptName = "Visualized" + selectedFileType;
            script =
$@"
using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.Trees;

public class {scriptName} : VisualizedBranch
{{
{bodyFields}
{getInstanceMethod}
}}";

            return script;
        }

        private static string GetLeafScript(ParameterInfo[] constructorParameters, Type selectedFileType)
        {
            var script = string.Empty;
            var scriptName = string.Empty;
            var bodyFields = string.Empty;
            var getInstanceMethod = string.Empty;

            foreach (var p in constructorParameters)
            {
                if (p.ParameterType == typeof(AIBehaviourGraph))
                    continue;
                bodyFields += $"{TAB}public {p.ParameterType} {p.Name};\n";
            }

            var paramsForMethod = string.Empty;
            for (int i = 0; i < constructorParameters.Length; i++)
            {
                paramsForMethod += $"{constructorParameters[i].ParameterType} {constructorParameters[i].Name}";
                if (i < constructorParameters.Length - 1)
                    paramsForMethod += ", ";
            }

            var paramsForMethodWithoutType = string.Empty;
            for (int i = 0; i < constructorParameters.Length; i++)
            {
                paramsForMethodWithoutType += $"{constructorParameters[i].Name}";
                if (i < constructorParameters.Length - 1)
                    paramsForMethodWithoutType += ", ";
            }


            getInstanceMethod =
$@"
{TAB}public override Leaf GetInstance()
{TAB}{{
{TAB}{TAB}return new {selectedFileType}({paramsForMethodWithoutType});
{TAB}}}";

            scriptName = "Visualized" + selectedFileType;
            script =
$@"
using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.Trees;

public class {scriptName} : VisualizedLeaf
{{
{bodyFields}
{getInstanceMethod}
{TAB}public override HierarchyBranch GetInstance(AIBehaviourGraph graph)
{TAB}{{
{TAB}    throw new System.NotImplementedException();
{TAB}}}
}}";

            return script;
        }
    }
}
