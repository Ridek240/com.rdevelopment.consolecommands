using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public class Parsers
{
    public static readonly List<IArgumentParser> _parsers = new()
    {
        new PrimitiveParser(),
        new GameObjectParser(),
        new BoolParser(),
        new Vector3Parser(),
        new Vector2Parser()
    };

    public static object Parse(string input, Type type)
    {
        foreach (var parser in _parsers)
        {
            if (parser.CanParse(type))
                return parser.Parse(input, type);
        }

        throw new Exception($"Brak parsera dla typu {type.Name}");
    }
}


public interface IArgumentParser
    {
        public bool CanParse(Type type);
        public object Parse(string input, Type targetType);
    }

    public class PrimitiveParser : IArgumentParser
    {
        public bool CanParse(Type type)
        {
            return type == typeof(int) || type == typeof(float) ||
                   type == typeof(double) || type == typeof(string);
        }

        public object Parse(string input, Type targetType)
        {
            if (targetType == typeof(string)) return input;
            if (targetType == typeof(int)) return int.Parse(input);
            if (targetType == typeof(float)) return float.Parse(input, CultureInfo.InvariantCulture);
            if (targetType == typeof(double)) return double.Parse(input);
            throw new InvalidOperationException();
        }
    }

    public class GameObjectParser : IArgumentParser
    {
        public bool CanParse(Type type) => type == typeof(GameObject);

    public object Parse(string input, Type _)
    {
        var allObjects = UnityEngine.Object.FindObjectsByType(typeof(GameObject), FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            if (obj.name.Replace(' ', '_') == input)
            {
                return obj;
            }
        }

        return null;
    }
    }

    public class BoolParser : IArgumentParser
    {
        public bool CanParse(Type type) => type == typeof(bool);

        public object Parse(string input, Type _) => bool.Parse(input);
    }
public class Vector3Parser : IArgumentParser
{
    public bool CanParse(Type type) => type == typeof(Vector3);

    public object Parse(string input, Type targetType)
    {
        var numbers = input.Split(',');
        if (numbers.Length != 3) throw new Exception($"Vectro3 Wymaga 3 liczb");

        return new Vector3(float.Parse(numbers[0], CultureInfo.InvariantCulture), float.Parse(numbers[1], CultureInfo.InvariantCulture), float.Parse(numbers[2], CultureInfo.InvariantCulture));
    }
}
public class Vector2Parser : IArgumentParser
{
    public bool CanParse(Type type) => type == typeof(Vector2);

    public object Parse(string input, Type targetType)
    {
        var numbers = input.Split(',');
        if (numbers.Length != 2) throw new Exception($"Vector2 Wymaga 2 liczb");

        return new Vector2(float.Parse(numbers[0], CultureInfo.InvariantCulture), float.Parse(numbers[1], CultureInfo.InvariantCulture));
    }
}

