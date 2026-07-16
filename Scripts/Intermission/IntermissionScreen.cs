using UnityEngine;

public class IntermissionScreen : MonoBehaviour
{
    private static IntermissionScreen instance;

    [Header("Display")]
    [SerializeField] private float showDuration = 5f;
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = new Color(0.9f, 0.7f, 0.3f);

    private static bool isVisible;
    private static float showStartTime;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Show()
    {
        isVisible = true;
        showStartTime = Time.time;
    }

    public static void Hide()
    {
        isVisible = false;
    }

    private void OnGUI()
    {
        if (!isVisible) return;
        if (LevelStats.Instance == null) return;

        float w = 400;
        float h = 300;
        float x = Screen.width / 2f - w / 2f;
        float y = Screen.height / 2f - h / 2f;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = fontSize + 10;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = textColor;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = fontSize;
        labelStyle.normal.textColor = textColor;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle bigStyle = new GUIStyle(GUI.skin.label);
        bigStyle.fontSize = fontSize + 6;
        bigStyle.fontStyle = FontStyle.Bold;
        bigStyle.normal.textColor = Color.white;
        bigStyle.alignment = TextAnchor.MiddleCenter;

        // background
        GUI.Box(new Rect(x, y, w, h), "");

        // title
        GUI.Label(new Rect(x, y + 10, w, 40), "LEVEL COMPLETE", titleStyle);

        // stats
        var stats = LevelStats.Instance;
        GUI.Label(new Rect(x, y + 60, w, 30), $"Map: {stats.name}", labelStyle);
        GUI.Label(new Rect(x, y + 95, w, 30), $"Time: {stats.GetTimeString()}", labelStyle);
        GUI.Label(new Rect(x, y + 130, w, 30), $"Enemies: {stats.enemiesKilled}/{stats.totalEnemies}", labelStyle);
        GUI.Label(new Rect(x, y + 165, w, 30), $"Secrets: {stats.secretsFound}/{stats.totalSecrets}", labelStyle);

        // completion
        float pct = stats.GetCompletionPercent();
        GUI.Label(new Rect(x, y + 210, w, 40), $"Completion: {pct:F0}%", bigStyle);

        // progress bar
        float barW = w - 40;
        GUI.Box(new Rect(x + 20, y + 250, barW, 16), "");
        GUI.Box(new Rect(x + 20, y + 250, barW * pct / 100f, 16), "");
    }
}