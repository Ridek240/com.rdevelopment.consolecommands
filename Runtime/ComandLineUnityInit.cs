using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ConsoleCommands.DebugSystem
{
    public class ComandLineUnityInit : MonoBehaviour
    {
        static ScrollRect scrollRect;
        static RectTransform contentRect;
        static Scrollbar scrollbar;
        static GameObject ConsoleSpace;
        static TMP_InputField CommandLine;
        static TextMeshProUGUI output;
        static List<string> commands = new List<string>();

        private static InputActionMap customMap;
        private static InputAction confirmAction;
        private static InputAction OpenAction;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            if (customMap == null)
            {
                customMap = new InputActionMap("Debug");
            }

            if (OpenAction == null)
            {
                OpenAction = customMap.AddAction("Open", InputActionType.Button);
                OpenAction.AddBinding("<Keyboard>/backquote");
            }

            if (confirmAction == null)
            {
                confirmAction = customMap.AddAction("Confirm", InputActionType.Button);
                confirmAction.AddBinding("<Keyboard>/enter");
            }
            OpenAction.performed -= ChangeConsoleActivation;
            confirmAction.performed -= ConfirmCommandActivation;

            OpenAction.performed += ChangeConsoleActivation;
            confirmAction.performed += ConfirmCommandActivation;
            customMap.Enable();
            GameObject canvasGO = new GameObject("DynamicCanvas");
            ConsoleSpace = canvasGO;
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGO);

            // === EventSystem (potrzebny dla InputField) ===

            if (FindObjectsByType(typeof(UnityEngine.EventSystems.EventSystem), FindObjectsSortMode.None).Length == 0)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // === InputField (TextMeshPro) ===
            GameObject inputGO = new GameObject("InputField");
            inputGO.transform.SetParent(canvasGO.transform);
            RectTransform inputRT = inputGO.AddComponent<RectTransform>();
            inputRT.anchorMin = new Vector2(0, 1);
            inputRT.anchorMax = Vector2.one;
            inputRT.pivot = new Vector2(0.5f, 1);
            inputRT.offsetMin = new Vector2(0, -40);
            inputRT.offsetMax = Vector2.zero;

            TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();
            inputField.textViewport = CreateViewport(inputGO.transform);
            inputField.textComponent = CreateText(inputField.textViewport.transform, "Command Line...", 14);
            CommandLine = inputField;
            Image inputImage = inputGO.AddComponent<Image>();
            inputImage.color = Color.white;
            inputField.targetGraphic = inputImage;

            GameObject scrollGO = new GameObject("ConsoleScrollRect", typeof(RectTransform));
            scrollGO.AddComponent<RectMask2D>();
            scrollGO.transform.SetParent(canvas.transform);

            RectTransform scrollRectTransform = scrollGO.GetComponent<RectTransform>();
            scrollRectTransform.anchoredPosition = new Vector2(0, -50);
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = Vector2.zero;
            scrollRectTransform.offsetMax = new Vector2(0, -50);

            scrollRect = scrollGO.AddComponent<ScrollRect>();
            Image scrollImage = scrollGO.AddComponent<Image>();
            scrollImage.color = new Color(0, 0, 0, 0.8f); // lekko czarne t³o

            scrollRect.horizontal = false;

            GameObject contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(scrollGO.transform);
            contentRect = contentGO.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup layoutGroup = contentGO.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.spacing = 2;

            ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;

            GameObject textGO = new GameObject("OutputText", typeof(RectTransform));
            textGO.transform.SetParent(contentGO.transform);
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 1);
            textRect.sizeDelta = new Vector2(0, 30);

            output = textGO.AddComponent<TextMeshProUGUI>();
            output.fontSize = 18;

            output.textWrappingMode = TextWrappingModes.Normal;
            output.overflowMode = TextOverflowModes.Truncate;
            output.text = "";

            // Stwórz Scrollbar po lewej stronie
            GameObject scrollbarGO = new GameObject("Scrollbar", typeof(RectTransform));
            scrollbarGO.transform.SetParent(scrollGO.transform);

            RectTransform scrollbarRect = scrollbarGO.GetComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(0.95f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.offsetMin = Vector2.zero;
            scrollbarRect.offsetMax = Vector2.zero;

            scrollbar = scrollbarGO.AddComponent<Scrollbar>();

            // Dodaj grafikê t³a i sliding area do Scrollbar
            GameObject backgroundGO = new GameObject("Background", typeof(RectTransform));
            backgroundGO.transform.SetParent(scrollbarGO.transform);
            RectTransform bgRect = backgroundGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = backgroundGO.AddComponent<Image>();
            bgImage.color = Color.gray;

            GameObject slidingAreaGO = new GameObject("SlidingArea", typeof(RectTransform));
            slidingAreaGO.transform.SetParent(scrollbarGO.transform);
            RectTransform slidingRect = slidingAreaGO.GetComponent<RectTransform>();
            slidingRect.anchorMin = Vector2.zero;
            slidingRect.anchorMax = Vector2.one;
            slidingRect.offsetMin = Vector2.zero;
            slidingRect.offsetMax = Vector2.zero;

            GameObject handleGO = new GameObject("Handle", typeof(RectTransform));
            handleGO.transform.SetParent(slidingAreaGO.transform);
            RectTransform handleRect = handleGO.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(0, 30);
            handleRect.anchorMin = new Vector2(0, 1);
            handleRect.anchorMax = new Vector2(1, 1);
            handleRect.pivot = new Vector2(0.5f, 1);
            handleRect.anchoredPosition = Vector2.zero;

            Image handleImage = handleGO.AddComponent<Image>();
            handleImage.color = Color.white;

            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRect;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            ConsoleSpace.SetActive(false);

            GameObject DebugInfo = new GameObject("DebugInfo");
            var cnavas = DebugInfo.AddComponent<Canvas>();
            cnavas.renderMode = RenderMode.ScreenSpaceOverlay;
            var text = DebugInfo.AddComponent<TextMeshProUGUI>();
            var debugmenu = DebugInfo.AddComponent<DebugMenu>();
            debugmenu.debugText = text;
            cnavas.sortingOrder = -100;
            DontDestroyOnLoad(DebugInfo);

        }

        private static RectTransform CreateViewport(Transform parent)
        {
            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(parent);
            RectTransform rt = viewportGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Mask mask = viewportGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            Image img = viewportGO.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.1f);

            return rt;
        }

        private static TMP_Text CreateText(Transform parent, string text, int size, bool isPlaceholder = false)
        {
            GameObject textGO = new GameObject(isPlaceholder ? "Placeholder" : "Text");
            textGO.transform.SetParent(parent);
            RectTransform rt = textGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TMP_Text tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = isPlaceholder ? Color.gray : Color.black;

            return tmp;
        }

        private static void ChangeConsoleActivation(InputAction.CallbackContext obj)
        {
            try
            {
                ConsoleSpace.SetActive(!ConsoleSpace.activeSelf);
                CommandLine.ActivateInputField();
                CommandLine.Select();

            }
            catch 
            { 
                Initialize();
            }
        }

        private static void ConfirmCommandActivation(InputAction.CallbackContext context)
        {
            var result = CommandRegistry.Execute(CommandLine.text);
            commands.Add(">>" + CommandLine.text);
            commands.Add("<<" + result);
            scrollRect.normalizedPosition = new Vector2(1, 0);
            output.text = string.Join("\n", commands.ToArray());
            CommandLine.ActivateInputField();
            CommandLine.Select();
        }


    }
}