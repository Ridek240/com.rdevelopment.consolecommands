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
    }
}