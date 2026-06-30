using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[ExecuteAlways]
public class MenuSceneController : MonoBehaviour
{
    [SerializeField] Sprite menuBackground;

    GameObject optionsPanel;

    void OnEnable()
    {
        BuildScene();
    }

    void Awake()
    {
        BuildScene();
    }

    void Start()
    {
        BuildScene();
        WireButtons();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            BuildScene();
        }
    }
#endif

    void BuildScene()
    {
        SceneUiFactory.EnsureCamera(new Color(0.38f, 0.68f, 0.88f, 1f));
        SceneUiFactory.EnsureEventSystem();

        Canvas canvas = SceneUiFactory.EnsureCanvas("Menu Canvas");
        Font font = SceneUiFactory.LoadFont();
        Transform root = canvas.transform;

        SceneUiFactory.EnsureFullScreenImage(root, "Wallpaper Background", menuBackground, new Color(0.38f, 0.68f, 0.88f, 1f));
        SceneUiFactory.RemoveChild(root, "Menu Panel");

        SceneUiFactory.EnsureText(root, "Title", "SNOW BOARDER", font, 44, TextAnchor.MiddleCenter, new Vector2(0f, 132f), new Vector2(500f, 64f));

        SceneUiFactory.EnsureText(root, "Subtitle", "Downhill arcade challenge", font, 18, TextAnchor.MiddleCenter, new Vector2(0f, 90f), new Vector2(500f, 34f));

        SceneUiFactory.EnsureButton(root, "Start Button", "Start", font, new Vector2(0f, 24f));
        SceneUiFactory.EnsureButton(root, "Options Button", "Options", font, new Vector2(0f, -42f));
        SceneUiFactory.EnsureButton(root, "Quit Button", "Quit", font, new Vector2(0f, -108f));

        optionsPanel = SceneUiFactory.EnsurePanel(root, "Controls Panel", new Color(0.01f, 0.2f, 0.25f, 0.96f), new Vector2(470f, 96f), new Vector2(0f, -176f));
        SceneUiFactory.EnsureText(optionsPanel.transform, "Controls Text",
            "A/D or Arrows: steer and flip   W/Up: move faster\nS/Down: brake   Space: boost   P/Esc: pause   R: restart",
            font, 16, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(430f, 76f));
        optionsPanel.SetActive(!Application.isPlaying);
    }

    void WireButtons()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Button start = SceneUiFactory.FindButton("Start Button");
        Button options = SceneUiFactory.FindButton("Options Button");
        Button quit = SceneUiFactory.FindButton("Quit Button");

        if (start != null)
        {
            start.onClick.RemoveAllListeners();
            start.onClick.AddListener(SceneFlow.LoadGame);
        }

        if (options != null)
        {
            options.onClick.RemoveAllListeners();
            options.onClick.AddListener(() =>
            {
                if (optionsPanel != null)
                {
                    optionsPanel.SetActive(!optionsPanel.activeSelf);
                }
            });
        }

        if (quit != null)
        {
            quit.onClick.RemoveAllListeners();
            quit.onClick.AddListener(SceneUiFactory.QuitGame);
        }
    }
}

public static class SceneUiFactory
{
    public static Canvas EnsureCanvas(string name)
    {
        GameObject canvasObject = GameObject.Find(name);
        if (canvasObject == null)
        {
            canvasObject = new GameObject(name, typeof(RectTransform));
        }

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = canvasObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvasObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        return canvas;
    }

    public static void EnsureCamera(Color background)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            camera = Object.FindAnyObjectByType<Camera>();
        }

        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            TrySetTag(cameraObject, "MainCamera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = background;
        camera.orthographic = true;
        camera.orthographicSize = 5f;
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
        eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
    }

    public static Font LoadFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Font.CreateDynamicFontFromOSFont("Arial", 16);
    }

    public static GameObject EnsurePanel(Transform parent, string name, Color color, Vector2 size, Vector2 position)
    {
        GameObject panel = FindOrCreateChild(parent, name, out bool created);
        Image image = panel.GetComponent<Image>();
        bool addedImage = image == null;
        if (image == null)
        {
            image = panel.AddComponent<Image>();
        }

        if (created || addedImage)
        {
            image.color = color;

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        return panel;
    }

    public static Image EnsureFullScreenImage(Transform parent, string name, Sprite sprite, Color fallbackColor)
    {
        GameObject imageObject = FindOrCreateChild(parent, name, out bool created);
        imageObject.transform.SetAsFirstSibling();

        Image image = imageObject.GetComponent<Image>();
        bool addedImage = image == null;
        if (image == null)
        {
            image = imageObject.AddComponent<Image>();
        }

        if (created || addedImage)
        {
            image.sprite = sprite;
            image.color = sprite != null ? Color.white : fallbackColor;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;

            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else if (image.sprite == null && sprite != null)
        {
            image.sprite = sprite;
        }

        if (sprite != null)
        {
            AspectRatioFitter fitter = imageObject.GetComponent<AspectRatioFitter>();
            bool addedFitter = fitter == null;
            if (fitter == null)
            {
                fitter = imageObject.AddComponent<AspectRatioFitter>();
            }

            if (created || addedFitter)
            {
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
            }
        }

        return image;
    }

    public static void RemoveChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(existing.gameObject);
        }
        else
        {
            Object.DestroyImmediate(existing.gameObject);
        }
    }

    public static Text EnsureText(Transform parent, string name, string value, Font font, int size, TextAnchor alignment, Vector2 position, Vector2 dimensions)
    {
        return EnsureText(parent, name, value, font, size, alignment, position, dimensions, out _);
    }

    public static Text EnsureText(Transform parent, string name, string value, Font font, int size, TextAnchor alignment, Vector2 position, Vector2 dimensions, out bool initialized)
    {
        GameObject textObject = FindOrCreateChild(parent, name, out bool created);
        Text text = textObject.GetComponent<Text>();
        bool addedText = text == null;
        if (text == null)
        {
            text = textObject.AddComponent<Text>();
        }

        initialized = created || addedText;

        if (initialized)
        {
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(12, size - 8);
            text.resizeTextMaxSize = size;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
        }
        else if (text.font == null)
        {
            text.font = font;
        }

        return text;
    }

    public static Button EnsureButton(Transform parent, string name, string label, Font font, Vector2 position)
    {
        GameObject buttonObject = FindOrCreateChild(parent, name, out bool created);

        Image image = buttonObject.GetComponent<Image>();
        bool addedImage = image == null;
        if (image == null)
        {
            image = buttonObject.AddComponent<Image>();
        }

        if (created || addedImage)
        {
            image.color = new Color(0.93f, 0.98f, 1f, 0.98f);
        }

        Button button = buttonObject.GetComponent<Button>();
        bool addedButton = button == null;
        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        if (created || addedButton)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.93f, 0.98f, 1f, 0.98f);
            colors.highlightedColor = new Color(0.76f, 0.9f, 1f, 1f);
            colors.pressedColor = new Color(0.54f, 0.78f, 0.96f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(250f, 50f);
        }

        Text labelText = EnsureText(buttonObject.transform, "Label", label, font, 22, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(250f, 50f), out bool labelInitialized);
        if (labelInitialized)
        {
            labelText.color = new Color(0.02f, 0.11f, 0.15f, 1f);
            RectTransform textRect = labelText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        return button;
    }

    public static Button FindButton(string name)
    {
        GameObject buttonObject = GameObject.Find(name);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    public static void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }
#endif

        Application.Quit();
    }

    static GameObject FindOrCreateChild(Transform parent, string name)
    {
        return FindOrCreateChild(parent, name, out _);
    }

    static GameObject FindOrCreateChild(Transform parent, string name, out bool created)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            created = false;
            return existing.gameObject;
        }

        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        created = true;
        return gameObject;
    }

    static void TrySetTag(GameObject gameObject, string tag)
    {
        try
        {
            gameObject.tag = tag;
        }
        catch (UnityException)
        {
            gameObject.tag = "Untagged";
        }
    }
}
