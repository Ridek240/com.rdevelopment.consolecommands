using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ComandLineUnityInit : MonoBehaviour
{


    static ScrollRect scrollRect;
    static RectTransform contentRect;
    static Scrollbar scrollbar;
    static GameObject ConsoleSpace;
    static TMP_InputField CommandLine;
    static TextMeshProUGUI output;
    static List<string> commands = new List<string>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private static InputActionMap customMap;
    private static InputAction confirmAction;
    private static InputAction OpenAction;

    [RuntimeInitializeOnLoadMethod]
    public static void Initialize()
    {

        customMap = new InputActionMap("Debug");

        // === Tworzymy akcjê Confirm ===
        OpenAction = customMap.AddAction("Open", InputActionType.Button);
        confirmAction = customMap.AddAction("Confirm", InputActionType.Button);
        OpenAction.AddBinding("<Keyboard>/backquote");
        confirmAction.AddBinding("<Keyboard>/enter");



        OpenAction.performed += ChangeConsoleActivation;
        confirmAction.performed += ConfirmCommandActivation;
        customMap.Enable();
        GameObject canvasGO = new GameObject("DynamicCanvas");
        ConsoleSpace = canvasGO;
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // === EventSystem (potrzebny dla InputField) ===
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // === InputField (TextMeshPro) ===
        GameObject inputGO = new GameObject("InputField");
        inputGO.transform.SetParent(canvasGO.transform);
        RectTransform inputRT = inputGO.AddComponent<RectTransform>();
        inputRT.anchorMin = new Vector2 (0, 1); 
        inputRT.anchorMax = Vector2.one;
        inputRT.pivot = new Vector2(0.5f,1);
        inputRT.offsetMin = new Vector2(0,-40);
        inputRT.offsetMax = Vector2.zero;
        //inputRT.sizeDelta = new Vector2(300, 40);
        //inputRT.anchoredPosition = new Vector2(0, 50);
        //inputRT.localScale = Vector3.one;

        TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();
        inputField.textViewport = CreateViewport(inputGO.transform);
        inputField.textComponent = CreateText(inputField.textViewport.transform, "Command Line...", 14);
        CommandLine = inputField;
        Image inputImage = inputGO.AddComponent<Image>();
        inputImage.color = Color.white;
        inputField.targetGraphic = inputImage;


        // === Text ===
        /*
        GameObject textGO = new GameObject("OutputText");
        textGO.transform.SetParent(canvasGO.transform);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(400, 40);
        textRT.anchoredPosition = new Vector2(0, -50);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = new Vector2(0, -50);

        //textRT.localScale = Vector3.one;

        TextMeshProUGUI outputText = textGO.AddComponent<TextMeshProUGUI>();
        outputText.text = "Wynik:";
        outputText.fontSize = 18;
        outputText.alignment = TextAlignmentOptions.TopLeft;
        output = outputText;

        */
        // Stwórz ScrollRect GameObject
        GameObject scrollGO = new GameObject("ConsoleScrollRect", typeof(RectTransform));
        scrollGO.AddComponent<RectMask2D>();
        scrollGO.transform.SetParent(canvas.transform);

        RectTransform scrollRectTransform = scrollGO.GetComponent<RectTransform>();
        scrollRectTransform.anchoredPosition = new Vector2(0, -50);
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = new Vector2(0, -50);

        // Dodaj ScrollRect i Image do ScrollRect (t³o)
        scrollRect = scrollGO.AddComponent<ScrollRect>();
        Image scrollImage = scrollGO.AddComponent<Image>();
        scrollImage.color = new Color(0, 0, 0, 0.8f); // lekko czarne t³o

        scrollRect.horizontal = false;

        // Stwórz Content (container na tekst)
        GameObject contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(scrollGO.transform);
        contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        // Dodaj layout grupê i ContentSizeFitter do Content
        VerticalLayoutGroup layoutGroup = contentGO.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.spacing = 2;

        ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;

        // Stwórz TextMeshPro obiekt dla outputText
        GameObject textGO = new GameObject("OutputText", typeof(RectTransform));
        textGO.transform.SetParent(contentGO.transform);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 1);
        textRect.sizeDelta = new Vector2(0, 30);  // wysokoœæ pojedynczej linii

        output = textGO.AddComponent<TextMeshProUGUI>();
        output.fontSize = 18;
        output.enableWordWrapping = true;
        output.overflowMode = TMPro.TextOverflowModes.Truncate;
        output.text = "";

        // Stwórz Scrollbar po lewej stronie
        GameObject scrollbarGO = new GameObject("Scrollbar", typeof(RectTransform));
        scrollbarGO.transform.SetParent(scrollGO.transform);

        RectTransform scrollbarRect = scrollbarGO.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(0.95f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);  // 5% szerokoœci po lewej
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
        // === Obs³uga wpisania tekstu ===
        /*
        inputField.onValueChanged.AddListener((value) =>
        {
            outputText.text = "Wynik: " + value;
        });*/

    }

    private static void ConfirmCommandActivation(InputAction.CallbackContext context)
    {
        var result = CommandRegistry.Execute(CommandLine.text);
        commands.Add(">>"+CommandLine.text);
        commands.Add("<<"+result);
        scrollRect.normalizedPosition = new Vector2(1,0);
        output.text = string.Join("\n",commands.ToArray());
        CommandLine.ActivateInputField();
        CommandLine.Select();
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
        
        ConsoleSpace.SetActive(!ConsoleSpace.activeSelf);

    }
}
