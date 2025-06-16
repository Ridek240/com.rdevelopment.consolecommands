using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class CommandAttribute : Attribute
{
    public string Name { get; }

    public CommandAttribute(string name)
    {
        Name = name;
    }
}


public class CommandRegistry
{
    private static readonly Dictionary<string, List<(MethodInfo method, Type declaringType, object target)>> _commands = new();

    [RuntimeInitializeOnLoadMethod]
    private static void LoadCommands()
    {
        _commands.Clear();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.FullName.StartsWith("Assembly-CSharp")) continue;
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsGenericType) continue;

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<CommandAttribute>();
                    if (attr == null) continue;

                    var commandName = attr.Name.ToLower();

                    if (_commands.ContainsKey(commandName))
                    {
                        Debug.LogWarning($"Komenda '{commandName}' ju¿ zarejestrowana — nadpisano.");
                    }
                    var instance = type.GetProperties(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(p => p.PropertyType == type);
                    if (!_commands.ContainsKey(commandName))
                        _commands[commandName] = new List<(MethodInfo method, Type declaringType, object target)>();

                    _commands[commandName].Add((method, type, instance));

                    //_commands[commandName] = (method, type, instance);
                }
            }
        }
        Debug.Log($"Zarejestrowano {_commands.Count} komend.");
    }

    public static string Execute(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "Komeda jest za krótka";

        var commandName = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        if (_commands.TryGetValue(commandName, out var entry))
        {


            foreach (var element in entry)
            {

                var (method, type, target) = element;
                var parameters = method.GetParameters();
                object methotTarget = null;
                if(target is PropertyInfo property)
                {
                    methotTarget = property.GetValue(null);
                }
                if (!method.IsStatic && methotTarget == null)
                {
                    methotTarget = FindObjectByNameAndType(type, args[0]);
                    if (methotTarget == null)
                    {
                        continue;
                        //return $"Target object {args[0]} do not exist. This Command Requers targetObject";
                    }
                    args = args.Skip(1).ToArray();
                }
                if (parameters.Length != args.Length)
                {
                    continue;
                    //return "Nieprawid³owa liczba argumentów.";

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
            return "Nie znalezionio przeci¹rzenia";

            //return method.Invoke(target, parsedArgs).ToString();
        }
        else
        {
            return "Nieznana komenda.";
        }
    }

    private static string InvokeCommand(string input, MethodInfo method, object methotTarget, object[] parsedArgs)
    {
        try
        {

            var result = method.Invoke(methotTarget, parsedArgs);
            if (method.ReturnType != typeof(void))
            {
                return $"Wynik komendy '{input}': {result}";
            }
            return "Complete";
        }
        catch (TargetInvocationException tie)
        {
            Debug.LogError($"B³¹d podczas wykonywania komendy '{input}': {tie.InnerException?.Message}");
            Debug.LogException(tie.InnerException ?? tie);
            return $"B³¹d podczas wykonywania komendy '{input}': {tie.InnerException?.Message}";
        }
        catch (Exception ex)
        {
            Debug.LogError($"B³¹d systemu komend: {ex.Message}");
            Debug.LogException(ex);
            return $"B³¹d systemu komend: {ex.Message}";
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
        return "\n"+string.Join("\n", commands.ToArray());
    }

}
