using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ConsoleCommands.Parser;
using ConsoleCommands.Commands;
using System.Text;


public class CommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public CommandAttribute(string name)
    {
        Name = name;
    }
    public CommandAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

namespace ConsoleCommands
{


    public class CommandRegistry
    {
        private static readonly Dictionary<string, List<(MethodInfo method, Type declaringType, object target)>> _commands = new();

        [RuntimeInitializeOnLoadMethod]
        private static void LoadCommands()
        {
            _commands.Clear();
            LoadCommandFromClass(typeof(BasicCommands));
            LoadCommandFromClass(typeof(CommandRegistry));
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string asmName = assembly.GetName().Name;
                if (asmName != "Assembly-CSharp" && asmName != "com.rdevelopment.consolecommands.Runtime")
                    continue;
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsGenericType) continue;

                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    foreach (var method in methods)
                    {
                        var attr = method.GetCustomAttribute<CommandAttribute>();
                        if (attr == null) continue;

                        var commandName = attr.Name.ToLower();

                        var instance = type.GetProperties(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(p => p.PropertyType == type);
                        if (!_commands.ContainsKey(commandName))
                            _commands[commandName] = new List<(MethodInfo method, Type declaringType, object target)>();

                        _commands[commandName].Add((method, type, instance));

                    }
                }
            }
            Debug.Log($"{_commands.Count} commends in assebly detected.");
        }
        private static void LoadCommandFromClass(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<CommandAttribute>();
                if (attr == null) continue;

                var commandName = attr.Name.ToLower();

                var instance = type.GetProperties(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(p => p.PropertyType == type);
                if (!_commands.ContainsKey(commandName))
                    _commands[commandName] = new List<(MethodInfo method, Type declaringType, object target)>();

                _commands[commandName].Add((method, type, instance));

            }
        }
        public static string Execute(string input)
        {
            var parts = SmartSplit(input);
            if (parts.Length == 0) return "Command is too short";

            var commandName = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            for(var i = 0; i < args.Length; i++)
            {
                args[i] = args[i].Replace("{","");
                args[i] = args[i].Replace("}","");
            }

            if (_commands.TryGetValue(commandName, out var entry))
            {
                foreach (var element in entry)
                {
                    var (method, type, target) = element;
                    var parameters = method.GetParameters();
                    object methotTarget = null;
                    if (target is PropertyInfo property)
                    {
                        methotTarget = property.GetValue(null);
                    }
                    if (!method.IsStatic && methotTarget == null)
                    {
                        methotTarget = FindObjectByNameAndType(type, args[0]);
                        if (methotTarget == null)
                        {
                            continue;
                        }
                        args = args.Skip(1).ToArray();
                    }
                    if (parameters.Length != args.Length)
                    {
                        continue;
                    }
                    try
                    {
                        object[] parsedArgs = new object[args.Length];
                        for (int i = 0; i < args.Length; i++)
                        {
                            var paramType = parameters[i].ParameterType;
                            parsedArgs[i] = Parsers.Parse(args[i], paramType);
                        }

                        return InvokeCommand(input, method, methotTarget, parsedArgs);
                    }
                    catch { continue; }
                }
                return "There is no command that meets the requirements";
            }
            else
            {
                return "Unnown command.";
            }
        }

        public static string[] SmartSplit(string input)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool insideBraces = false;

            foreach (char c in input)
            {
                if (c == '{')
                {
                    insideBraces = true;
                    current.Append(c);
                }
                else if (c == '}')
                {
                    insideBraces = false;
                    current.Append(c);
                }
                else if (char.IsWhiteSpace(c) && !insideBraces)
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            if (current.Length > 0)
                result.Add(current.ToString());
            return result.ToArray();
        }
        private static string InvokeCommand(string input, MethodInfo method, object methotTarget, object[] parsedArgs)
        {
            try
            {
                var result = method.Invoke(methotTarget, parsedArgs);
                if (method.ReturnType != typeof(void))
                {
                    return $"Result '{input}': {result}";
                }
                return "Complete";
            }
            catch (TargetInvocationException tie)
            {
                Debug.LogError($"Error During Command Execution '{input}': {tie.InnerException?.Message}");
                Debug.LogException(tie.InnerException ?? tie);
                return $"Error During Command Execution '{input}': {tie.InnerException?.Message}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"Command system error: {ex.Message}");
                Debug.LogException(ex);
                return $"Command system error: {ex.Message}";
            }
        }

        public static UnityEngine.Object FindObjectByNameAndType(Type type, string objectName)
        {
            var allObjects = UnityEngine.Object.FindObjectsByType(type, FindObjectsSortMode.None);

            foreach (var obj in allObjects)
            {
                if (((Component)obj).gameObject.name.Replace(' ', '_') == objectName)
                {
                    return obj;
                }
            }

            return null;
        }
        [Command("Help")]
        public static string Help()
        {
            List<string> commands = new List<string>();
            foreach (var entry in _commands)
            {
                string commandName = entry.Key;

                foreach (var arg in entry.Value)
                {
                    MethodInfo method = arg.method;
                    ParameterInfo[] parameters = method.GetParameters();

                    // Przygotuj listê argumentów w formacie: "arg1:Type, arg2:Type"
                    string args = string.Join(", ", parameters.Select(p => $"{p.Name}:{p.ParameterType.Name}"));

                    bool needsTarget = arg.target == null && !arg.method.IsStatic;
                    string targetHint = needsTarget ? "Target:Name " : "";

                    commands.Add($"{commandName} {targetHint}{args}");
                }
            }
            return "\n" + string.Join("\n", commands.ToArray());
        }

    }
}
