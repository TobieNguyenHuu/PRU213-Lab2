using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class GameHud : MonoBehaviour
{
    Text scoreText;
    Text speedText;
    Text livesText;
    Text comboText;
    Image boostEnergyFill;
    RectTransform boostEnergyFillRect;
    Text messageText;
    GameObject messagePanel;

    public static GameHud Ensure()
    {
        GameHud existing = FindAnyObjectByType<GameHud>();
        if (existing != null)
        {
            return existing;
        }

        GameObject canvasObject = new GameObject("Runtime HUD", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameHud hud = canvasObject.AddComponent<GameHud>();
        hud.Build(canvasObject.transform);
        EnsureEventSystem();
        return hud;
    }

    static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
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

    void Build(Transform root)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        }

        scoreText = CreateText(root, "Score", font, 22, TextAnchor.UpperLeft, new Vector2(14f, -12f), new Vector2(360f, 30f));
        speedText = CreateText(root, "Speed", font, 20, TextAnchor.UpperLeft, new Vector2(14f, -42f), new Vector2(360f, 28f));
        livesText = CreateText(root, "Lives", font, 20, TextAnchor.UpperLeft, new Vector2(14f, -70f), new Vector2(360f, 28f));
        comboText = CreateText(root, "Combo", font, 20, TextAnchor.UpperLeft, new Vector2(14f, -98f), new Vector2(460f, 28f));
        boostEnergyFill = CreateBoostEnergyBar(root, new Vector2(14f, -132f), new Vector2(230f, 16f));
        boostEnergyFillRect = boostEnergyFill.rectTransform;

        messagePanel = new GameObject("Message Panel", typeof(RectTransform));
        messagePanel.transform.SetParent(root, false);
        Image messageBackground = messagePanel.AddComponent<Image>();
        messageBackground.color = new Color(0.02f, 0.08f, 0.12f, 0.84f);

        RectTransform panelRect = messagePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, 92f);
        panelRect.sizeDelta = new Vector2(760f, 54f);

        messageText = CreateText(messagePanel.transform, "Message", font, 26, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(740f, 44f));
        messageText.resizeTextForBestFit = true;
        messageText.resizeTextMinSize = 16;
        messageText.resizeTextMaxSize = 26;

    }

    void Update()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null)
        {
            return;
        }

        scoreText.text = $"Score: {manager.Score:000000}";
        speedText.text = $"Speed: {manager.CurrentSpeed:0.0}";
        livesText.text = $"Lives: {manager.Lives}";
        comboText.text = $"Combo: {manager.ComboCount}  x{manager.Multiplier:0.0}";
        float boostEnergy = manager.ManualBoostEnergy01;
        boostEnergyFillRect.localScale = new Vector3(boostEnergy, 1f, 1f);
        boostEnergyFill.color = Color.Lerp(
            new Color(1f, 0.28f, 0.24f, 0.95f),
            new Color(0.35f, 0.9f, 1f, 0.95f),
            boostEnergy);
        messageText.text = manager.StatusMessage;
        messagePanel.SetActive(!string.IsNullOrWhiteSpace(manager.StatusMessage));
    }

    static Image CreateBoostEnergyBar(Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject backgroundObject = new GameObject("Boost Energy Bar", typeof(RectTransform));
        backgroundObject.transform.SetParent(parent, false);

        Image background = backgroundObject.AddComponent<Image>();
        background.color = new Color(0.03f, 0.1f, 0.14f, 0.86f);
        background.raycastTarget = false;

        RectTransform backgroundRect = background.rectTransform;
        backgroundRect.anchorMin = new Vector2(0f, 1f);
        backgroundRect.anchorMax = new Vector2(0f, 1f);
        backgroundRect.pivot = new Vector2(0f, 1f);
        backgroundRect.anchoredPosition = anchoredPosition;
        backgroundRect.sizeDelta = sizeDelta;

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform));
        fillObject.transform.SetParent(backgroundObject.transform, false);

        Image fill = fillObject.AddComponent<Image>();
        fill.color = new Color(0.35f, 0.9f, 1f, 0.95f);
        fill.type = Image.Type.Simple;
        fill.raycastTarget = false;

        RectTransform fillRect = fill.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = new Vector2(0f, 0.5f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = new Vector2(2f, 0f);
        fillRect.sizeDelta = new Vector2(sizeDelta.x - 4f, sizeDelta.y - 4f);

        return fill;
    }

    static Text CreateText(Transform parent, string text, Font font, int size, TextAnchor alignment, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject textObject = new GameObject(text, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        Text uiText = textObject.AddComponent<Text>();
        uiText.font = font;
        uiText.fontSize = size;
        uiText.alignment = alignment;
        uiText.color = Color.white;
        uiText.text = text;
        uiText.raycastTarget = false;

        RectTransform rect = uiText.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        return uiText;
    }

}
