using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace ConsoleCommands.Commands
{
    
    public class BasicCommands
    {
        [Command("Teleport","Teleports object to cordinates")]
        public static void Teleport(GameObject target, float x, float y, float z)
        {
            target.transform.position = new Vector3(x, y, z);
        }
        [Command("Teleport", "Teleports object to vector")]
        public static void Teleport(GameObject target, Vector3 cordinates)
        {
            target.transform.position = cordinates;
        }
        [Command("Teleport", "Teleports object to another object")]
        public static void Teleport(GameObject from, GameObject to)
        {
            from.transform.position = to.transform.position;
        }
        [Command("Teleport", "Teleports object to another object with offset")]
        public static void Teleport(GameObject from, GameObject to, Vector3 offset)
        {
            from.transform.position = to.transform.position + offset;
        }

        [Command("SetActive", "Changes activation state of object")]
        public static void SetActive(GameObject gobject, bool state)
        {
            gobject.SetActive(state);
        }
        [Command("TimeScale","Changes speed of game")]
        public static void ChangeGameSpeed(float speed)
        {
            if(speed < 0)
            {
                speed = 0;
            }
            Time.timeScale = speed;
        }
        [Command("TimeDefault", "Changes game speed to deafault value")]
        public static void ChangeGameSpeed()
        {
            Time.timeScale = 1.0f;
        }
        [Command("TimeStop","Pauses Game")]
        public static void ChangeGameSpeedPause()
        {
            Time.timeScale = 0f;
        }
        [Command("Modify", "Modifies values in component")]
        public static void Modify(GameObject target, string type, string fields) 
        {
            Type t_type = FindTypeByName(type);
            if(t_type == null)
            {
                throw new Exception($"{type} could not be found in the assebly");
            }
            if (target.TryGetComponent(t_type, out var obiect))
            {
                var fields_values = ParseFields(fields);
                string message = "";
                foreach (var field in fields_values)
                {


                    FieldInfo fieldinfo = t_type.GetField(field.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (fieldinfo != null)
                    {
                        //var value = fieldinfo.GetValue(obiect);
                        fieldinfo.SetValue(obiect, Parser.Parsers.Parse(field.Value, fieldinfo.FieldType));
                    }
                    else
                    {
                        message += $"{field.Key} do not exits in {t_type.Name} \n";
                    }
                }
                if(message.Length > 0) 
                {
                    throw new Exception(message);
                }
            }
            else
            {
                throw new Exception($"{target.name} does not have {t_type.Name}");
            }
        }

        public static Dictionary<string, string> ParseFields(string input)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            // Rozdziel po przecinkach
            var pairs = input.Split(',');

            foreach (var pair in pairs)
            {
                var parts = pair.Split(':');
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                result[key] = value;
            }

            return result;
        }

        private static Type FindTypeByName(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);
        }
    }
}