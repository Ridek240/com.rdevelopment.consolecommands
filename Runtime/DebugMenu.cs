using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


namespace ConsoleCommands.Debug
{
    public class DebugMenu : MonoBehaviour
    {

        public TextMeshProUGUI debugText;
        public static State FPScounter = State.OFF;
        public static State Buttons = State.OFF;
        public static State Actions = State.OFF;

        [Command("FPS")]
        public static void Fps(State value)
        {
            FPScounter = value;
        }

        [Command("Buttons")]
        public static void ButtonsFun(State value)
        {
            Buttons = value;
        }

        [Command("Actions")]
        public static void ActionsFun(State value)
        {
            Actions = value;
        }
        [Command("Debug")]
        public static void Debug(State value)
        {
            Actions = value;
            Buttons = value;
            FPScounter = value;
        }
        void Update()
        {
            if (debugText == null) return;
            string debugTextStr = "";
            if (FPScounter != State.OFF)
            {
                float msec = Time.deltaTime * 1000.0f;
                float fps = 1.0f / Time.deltaTime;
                debugTextStr = "FPS: " + fps.ToString("0.") + " (" + msec.ToString("0.0") + " ms)\n";

            }
            if (FPScounter == State.DEBUG)
            {
                int vsync = QualitySettings.vSyncCount;
                debugTextStr += "VSync: " + (vsync > 0 ? "ON" : "OFF") + "\n";
            }
            if (Buttons != State.OFF)
            {
                string debugButtons = "Button Pressed:";
                if (Keyboard.current != null)
                {
                    foreach (var key in Keyboard.current.allKeys)
                    {
                        if (key.isPressed)
                            debugButtons += " " + key.name.ToUpper() + ",";
                    }
                }

                if (Mouse.current != null)
                {
                    foreach (var control in Mouse.current.allControls)
                    {
                        if (control is ButtonControl button && button.isPressed)
                            debugButtons += " " + control.name.ToUpper() + ",";
                    }
                }

                if (Gamepad.current != null)
                {
                    foreach (var control in Gamepad.current.allControls)
                    {
                        if (control is ButtonControl button && button.isPressed)
                            debugButtons += " " + control.name.ToUpper() + ",";
                    }

                    if (Gamepad.current.leftStick.ReadValue() != Vector2.zero)
                        debugButtons += " LEFTSTICK,";
                    if (Gamepad.current.rightStick.ReadValue() != Vector2.zero)
                        debugButtons += " RIGHTSTICK,";
                }

                debugTextStr += debugButtons + "\n";
            }
            if (Actions != State.OFF)
            {
                debugTextStr += "Active Actions:\n";
                foreach (var action in InputSystem.actions)
                {
                    if (action.enabled)
                    {
                        var value = action.ReadValueAsObject();
                        if (value != null)
                            debugTextStr += action.name + " = " + value.ToString() + "\n";
                    }
                }
            }
            debugText.text = debugTextStr;
        }

        public enum State
        {
            OFF,
            ON,
            DEBUG
        }
    }
}

