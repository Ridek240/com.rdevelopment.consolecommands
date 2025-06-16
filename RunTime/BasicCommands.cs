using UnityEngine;

public class BasicCommands
{
    [Command("Teleport")]
    public static void Teleport(GameObject target, float x, float y, float z)
    {
        target.transform.position = new Vector3(x, y, z);
    }
    [Command("Teleport")]
    public static void Teleport(GameObject target, Vector3 cordinates)
    {
        target.transform.position = cordinates;
    }
}
