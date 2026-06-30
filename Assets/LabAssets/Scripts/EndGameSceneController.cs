using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class EndGameSceneController : MonoBehaviour
{
    [SerializeField] Sprite endGameBackground;

    Text titleText;
    Text resultText;

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
        RefreshResult();
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            RefreshResult();
        }
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
        RunResult result = SceneFlow.LastResult;
        SceneUiFactory.EnsureCamera(result.Finished ? new Color(0.48f, 0.77f, 0.65f, 1f) : new Color(0.22f, 0.3f, 0.42f, 1f));
        SceneUiFactory.EnsureEventSystem();

        Canvas canvas = SceneUiFactory.EnsureCanvas("EndGame Canvas");
        Font font = SceneUiFactory.LoadFont();
        Transform root = canvas.transform;

        SceneUiFactory.EnsureFullScreenImage(root, "EndGame Background", endGameBackground, new Color(0.22f, 0.3f, 0.42f, 1f));
        SceneUiFactory.EnsurePanel(root, "EndGame Panel", new Color(0.02f, 0.08f, 0.12f, 0.58f), new Vector2(590f, 440f), Vector2.zero);
        titleText = SceneUiFactory.EnsureText(root, "Title", "GAME OVER", font, 42, TextAnchor.MiddleCenter, new Vector2(0f, 140f), new Vector2(520f, 62f));
        resultText = SceneUiFactory.EnsureText(root, "Result Text", string.Empty, font, 24, TextAnchor.MiddleCenter, new Vector2(0f, 48f), new Vector2(480f, 110f));
        resultText.lineSpacing = 1.25f;

        SceneUiFactory.EnsureButton(root, "Retry Button", "Retry", font, new Vector2(0f, -48f));
        SceneUiFactory.EnsureButton(root, "Main Menu Button", "Main Menu", font, new Vector2(0f, -112f));
        SceneUiFactory.EnsureButton(root, "Quit Button", "Quit", font, new Vector2(0f, -176f));

        RefreshResult();
    }

    void RefreshResult()
    {
        RunResult result = SceneFlow.LastResult;
        if (titleText != null)
        {
            titleText.text = result.Finished ? "RUN COMPLETE" : "GAME OVER";
        }

        if (resultText != null)
        {
            resultText.text = $"Score: {result.Score:000000}\nBest combo: {result.BestCombo}";
        }
    }

    void WireButtons()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Button retry = SceneUiFactory.FindButton("Retry Button");
        Button menu = SceneUiFactory.FindButton("Main Menu Button");
        Button quit = SceneUiFactory.FindButton("Quit Button");

        if (retry != null)
        {
            retry.onClick.RemoveAllListeners();
            retry.onClick.AddListener(SceneFlow.LoadGame);
        }

        if (menu != null)
        {
            menu.onClick.RemoveAllListeners();
            menu.onClick.AddListener(SceneFlow.LoadMenu);
        }

        if (quit != null)
        {
            quit.onClick.RemoveAllListeners();
            quit.onClick.AddListener(SceneUiFactory.QuitGame);
        }
    }
}
