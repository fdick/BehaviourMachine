using BehaviourGraph.Trees;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BehaviourGraph.Conditions;
using BehaviourGraph.States;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    public static class VisualizedForestObjectGenerator 
    {
        private const string TAB = "    ";


        [MenuItem("Tools/BehaviourMachine/Create Visualized Class")]
        public static void Generate()
        {
            if (Selection.activeObject == null)
                return;
            var selectedObj = Selection.activeObject;

            //find type in all assemblies
            var selectedFileType = AppDomain.CurrentDomain.GetAssemblies()
                .Reverse()
                .Select(assembly => assembly.GetType(selectedObj.name))
                .FirstOrDefault(t => t != null);
            
            if(selectedFileType == null)
                UnityEngine.Debug.Log($"I can't find a type of {selectedObj.name} in all assemblies!");

            if (!typeof(ITree).IsAssignableFrom(selectedFileType) &&
                !typeof(IState).IsAssignableFrom(selectedFileType) &&
                !typeof(ICondition).IsAssignableFrom(selectedFileType))
            {
                
                UnityEngine.Debug.Log($"<{selectedObj.name}> can't be origin for the generator!");
                return;
            }

            var scriptName = "Visualized" + selectedObj.name;
            var directory = $"{Application.dataPath}";
            var scriptPath = EditorUtility.SaveFilePanel("Save as", directory, scriptName, "cs");

            var constructors = selectedFileType.GetConstructors();

            if (constructors == null || constructors.Length == 0)
            {
                UnityEngine.Debug.Log($"<{selectedObj.name}> doesn't have any constructors!");
                return;
            }

            var constructorParameters = constructors[0].GetParameters();
            var script = string.Empty;

            if (typeof(ITree).IsAssignableFrom(selectedFileType))
                script = GetBranchScript(constructorParameters, selectedFileType);
            else if (typeof(ICondition).IsAssignableFrom(selectedFileType))
                script = GetCondiitonScript(constructorParameters, selectedFileType);
            else if (typeof(IState).IsAssignableFrom(selectedFileType))
                script = GetStateScript(constructorParameters, selectedFileType);


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
                if (typeof(IState).IsAssignableFrom(p.ParameterType))
                    paramsForMethodWithoutType += $"tree.GetStates()[{p.Name}_ID]";
                else 
                    paramsForMethodWithoutType += $"{constructorParameters[i].Name}";

                if (i < constructorParameters.Length - 1)
                    paramsForMethodWithoutType += ", ";
            }


            foreach (var p in constructorParameters)
            {
                if (typeof(IState).IsAssignableFrom(p.ParameterType))
                    bodyFields += $"{TAB}public int {p.Name}_ID;\n";
                else
                    bodyFields += $"{TAB}public {p.ParameterType} {p.Name};\n";
            }

            getInstanceMethod =
$@"
{TAB}public override ICondition GetInstance(Sequence seq)
{TAB}{{
{TAB}{TAB}return new {selectedFileType}({paramsForMethodWithoutType});
{TAB}}}";

            scriptName = "Visualized" + selectedFileType;

            script =
$@"
using BehaviourGraph.Visualizer;
using BehaviourGraph.Conditions;
using BehaviourGraph.States;

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
                if (p.ParameterType == typeof(BehaviourMachine))
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
{TAB}public ITree GetInstance(BehaviourMachine graph)
{TAB}{{
{TAB}{TAB}return new {selectedFileType}({paramsForMethodWithoutType});
{TAB}}}

{TAB}public override IState GetInstance(BehaviourMachine graph)
{TAB}{{
{TAB}{TAB}Debug.LogError($""{{this}} - is not a state!"");
{TAB}{TAB}return null;
{ TAB}}}
";

            scriptName = "Visualized" + selectedFileType;
            script =
$@"
using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.Trees;

public class {scriptName} : VisualizedState, IVisualizedTree
{{
{bodyFields}
{getInstanceMethod}
}}";

            return script;
        }

        private static string GetStateScript(ParameterInfo[] constructorParameters, Type selectedFileType)
        {
            var script = string.Empty;
            var scriptName = string.Empty;
            var bodyFields = string.Empty;
            var getInstanceMethod = string.Empty;

            foreach (var p in constructorParameters)
            {
                if (p.ParameterType == typeof(BehaviourMachine))
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
{TAB}public override IState GetInstance(BehaviourMachine graph)
{TAB}{{
{TAB}{TAB}return new {selectedFileType}({paramsForMethodWithoutType});
{TAB}}}";

            scriptName = "Visualized" + selectedFileType;
            script =
$@"
using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.States;

public class {scriptName} : VisualizedState
{{
{bodyFields}
{getInstanceMethod}
}}";

            return script;
        }
    }
}
