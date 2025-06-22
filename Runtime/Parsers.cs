using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;


namespace ConsoleCommands.Parser
{
    public class Parsers
    {
        public static readonly List<IArgumentParser> _parsers = new()
    {
        new PrimitiveParser(),
        new GameObjectParser(),
        new BoolParser(),
        new Vector3Parser(),
        new Vector2Parser(),
        new TransformParser(),
        new RectTransformParser(),
        new EnumParser()
    };

        public static object Parse(string input, Type type)
        {
            foreach (var parser in _parsers)
            {
                if (parser.CanParse(type))
                    return parser.Parse(input, type);
            }

            throw new Exception($"There is no parser for {type.Name}");
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
            if (numbers.Length != 3) throw new Exception($"Vector3 requiers 3 numbers");

            return new Vector3(float.Parse(numbers[0], CultureInfo.InvariantCulture), float.Parse(numbers[1], CultureInfo.InvariantCulture), float.Parse(numbers[2], CultureInfo.InvariantCulture));
        }
    }
    public class Vector2Parser : IArgumentParser
    {
        public bool CanParse(Type type) => type == typeof(Vector2);

        public object Parse(string input, Type targetType)
        {
            var numbers = input.Split(',');
            if (numbers.Length != 2) throw new Exception($"Vector2 requiers 2 numbers");

            return new Vector2(float.Parse(numbers[0], CultureInfo.InvariantCulture), float.Parse(numbers[1], CultureInfo.InvariantCulture));
        }
    }

    public class TransformParser : IArgumentParser
    {
        public bool CanParse(Type type) => type == typeof(Transform);

        public object Parse(string input, Type targetType)
        {
            return CommandRegistry.FindObjectByNameAndType(typeof(Transform), input);
        }
    }

    public class RectTransformParser : IArgumentParser
    {
        public bool CanParse(Type type) => type == typeof(RectTransform);

        public object Parse(string input, Type targetType)
        {
            return CommandRegistry.FindObjectByNameAndType(typeof(RectTransform), input);
        }
    }

    public class EnumParser : IArgumentParser
    {
        public bool CanParse(Type type) => type.IsEnum;

        public object Parse(string input, Type targetType)
        {
            if (!targetType.IsEnum)
                throw new ArgumentException("Target type must be an enum.");

            return Enum.Parse(targetType, input, ignoreCase: true);
        }
    }
}


