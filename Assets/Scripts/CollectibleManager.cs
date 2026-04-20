using UnityEngine;
using UnityEngine.UI;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }

    private int    _count;
    private int    _total;
    private Text   _label;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _total = FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length;
        BuildUI();
    }

    public void OnCollect()
    {
        _count++;
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (_label != null)
            _label.text = $"Flowers: {_count} / {_total}";
    }

    void BuildUI()
    {
        GameObject canvasGO = new GameObject("CollectibleCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Position the canvas floating in front of the player's start position
        canvasGO.transform.position = new Vector3(0f, 3.5f, -8f);
        canvasGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Background panel
        GameObject panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image bg = panelGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.45f);
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(300f, 60f);

        // Label
        GameObject textGO = new GameObject("CountLabel");
        textGO.transform.SetParent(panelGO.transform, false);
        _label = textGO.AddComponent<Text>();
        _label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _label.fontSize  = 28;
        _label.alignment = TextAnchor.MiddleCenter;
        _label.color     = Color.white;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.sizeDelta        = new Vector2(300f, 60f);
        textRect.anchoredPosition = Vector2.zero;

        UpdateLabel();
    }
}
