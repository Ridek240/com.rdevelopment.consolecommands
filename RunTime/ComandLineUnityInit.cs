using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ComandLineUnityInit : MonoBehaviour
{

    static GameObject ConsoleSpace;
    static TMP_InputField CommandLine;
    static TextMeshProUGUI output;
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
        // === Obs³uga wpisania tekstu ===
        /*
        inputField.onValueChanged.AddListener((value) =>
        {
            outputText.text = "Wynik: " + value;
        });*/

    }

    private static void ConfirmCommandActivation(InputAction.CallbackContext context)
    {
        output.text = CommandRegistry.Execute(CommandLine.text);
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
